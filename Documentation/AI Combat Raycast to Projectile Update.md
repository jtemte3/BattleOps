# AI Combat: Raycast to Projectile Update
Model: qwen/qwen3.6-27b
Created: 7/5/2026, 3:33:17 PM
Exported from: LM Studio 0.4.16+2

### User
Currently my AI Bot setup uses raycasts to simulate shooting. I want to update the AICombat script to spawn a physical projectile instead. The catch is, the bullet needs to have spread, but the muzzle gameobject cant be rotated. Can you give me an updated segment of the FireShot() function that uses a prefab "bullet" gameobject and keeps the spread feature?
- Assets/Scripts/Bots/AICombat.cs

### Assistant
The user wants me to update an AICombat script to change from raycast shooting to spawning a physical projectile. They want to:
1. Keep the spread feature
2. Use a prefab called "bullet"
3. The muzzle gameobject cannot be rotated (so we need to handle spread differently)

Let me first read the current AICombat.cs file to understand the existing FireShot() function and how spread is currently implemented.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "read",
  "parameters": {
    "filePath": "Assets/Scripts/Bots/AICombat.cs"
  },
  "pluginIdentifier": "843645386"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#type:file\n#offset:1\n#limit:326\n#total:326\n#has_more:false\n#content_bytes:9895\n1: using Unity.VisualScripting;\n2: using UnityEngine;\n3: using UnityEngine.VFX;\n4: \n5: /// <summary>\n6: /// Handles:\n7: /// - Target aiming\n8: /// - Fire timing\n9: /// - Burst fire\n10: /// - Accuracy/spread\n11: /// - Weapon integration\n12: /// - Procedural recoil\n13: /// \n14: /// Does NOT:\n15: /// - Control movement\n16: /// - Decide AI states\n17: /// - Handle perception\n18: /// </summary>\n19: public class AICombat : MonoBehaviour, IAIBehaviour\n20: {\n21:     public AIEntity entity;\n22: \n23:     [Header(\"References\")]\n24:     public Transform weaponMuzzle;\n25: \n26:     [Tooltip(\"Transform the rig aims toward\")]\n27:     public Transform weaponAimTarget;\n28: \n29:     public ProceduralWeaponMotion weaponMotion;\n30: \n31:     [Header(\"Combat\")]\n32:     public float attackRange = 25f;\n33: \n34:     [Tooltip(\"How quickly AI rotates aim target\")]\n35:     public float aimSpeed = 12f;\n36: \n37:     [Header(\"Fire Settings\")]\n38:     public bool automatic = true;\n39: \n40:     public float fireRate = 0.1f;\n41: \n42:     [Space]\n43:     public int burstCountMin = 2;\n44:     public int burstCountMax = 5;\n45:     [Space]\n46:     public float burstDelayMin = 0.08f;\n47:     public float burstDelayMax = 0.25f;\n48: \n49:     [Header(\"Accuracy\")]\n50:     [Tooltip(\"Lower = more accurate\")]\n51:     public float spreadRadius = 0.15f;\n52: \n53:     [Tooltip(\"Extra spread while moving\")]\n54:     public float movingSpreadMultiplier = 2f;\n55: \n56:     [Header(\"Search Fire\")]\n57:     public bool suppressLastKnownPosition = true;\n58:     public float suppressionDurationMin;\n59:     public float suppressionDurationMax;\n60:     private float endSuppressionTime;\n61: \n62:     [Tooltip(\"Chance to continue firing at last known position\")]\n63:     [Range(0f, 1f)]\n64:     public float suppressionFireChance = 0.5f;\n65: \n66:     [Header(\"Debug\")]\n67:     public bool debugDrawShots = true;\n68: \n69:     public float nextFireTime;\n70:     public float nextBurstTime;\n71: \n72:     public int burstCount;\n73:     public int currentBurstShots;\n74:     public bool isBursting;\n75: \n76:     public bool isSuppressing;\n77: \n78:     private float lightOffTime = 0f;\n79:     private float muzzleLightDuration = .1f;\n80: \n81:     public void Initialize(AIEntity entity)\n82:     {\n83:         this.entity = entity;\n84: \n85:         burstCount = Random.Range(burstCountMin, burstCountMax);\n86:     }\n87: \n88:     private void Update()\n89:     {\n90:         if (entity.currentState == AIState.Dead)\n91:         {\n92:             return;\n93:         }\n94: \n95:         switch (entity.currentState)\n96:         {\n97:             case AIState.Combat:\n98:                 HandleCombat();\n99:                 break;\n100: \n101:             case AIState.Suppress:\n102:                 if (isSuppressing == false)\n103:                 {\n104:                     isSuppressing = true;\n105:                     endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n106:                 }\n107:                 \n108:                 if (isSuppressing == true)\n109:                 {\n110:                     if (Time.time >= endSuppressionTime)\n111:                     {\n112:                         isSuppressing = false;\n113:                     }\n114:                     else\n115:                     {\n116:                         HandleSuppressionFire();\n117:                     }\n118:                 }\n119:                 break;\n120:         }\n121: \n122:         if (weaponMuzzle.GetComponent<Light>().enabled)\n123:         {\n124:             if (Time.time >= lightOffTime)\n125:             {\n126:                 weaponMuzzle.GetComponent<Light>().enabled = false;\n127:             }\n128:         }\n129:     }\n130: \n131:     // --------------------------------------------------\n132:     // COMBAT\n133:     // --------------------------------------------------\n134: \n135:     private void HandleCombat()\n136:     {\n137:         Transform target = entity.perception.currentTarget;\n138: \n139:         if (target == null)\n140:         {\n141:             return;\n142:         }\n143: \n144:         float distance = Vector3.Distance(transform.position, target.position);\n145: \n146:         UpdateAim(target.position);\n147: \n148:         if (distance <= attackRange)\n149:         {\n150:             TryFire(target.position);\n151:         }\n152:     }\n153: \n154:     // --------------------------------------------------\n155:     // SUPPRESSION FIRE\n156:     // --------------------------------------------------\n157: \n158:     private void HandleSuppressionFire()\n159:     {\n160:         if (!suppressLastKnownPosition)\n161:         {\n162:             return;\n163:         }\n164: \n165:         if (Random.value > suppressionFireChance)\n166:         {\n167:             return;\n168:         }\n169: \n170:         UpdateAim(\n171:             entity.perception.lastKnownPosition\n172:         );\n173: \n174:         TryFire(\n175:             entity.perception.lastKnownPosition\n176:         );\n177:     }\n178: \n179:     // --------------------------------------------------\n180:     // AIMING\n181:     // --------------------------------------------------\n182: \n183:     private void UpdateAim(Vector3 targetPosition)\n184:     {\n185:         if (weaponAimTarget == null)\n186:         {\n187:             return;\n188:         }\n189: \n190:         Vector3 current = weaponAimTarget.position;\n191: \n192:         Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);\n193: \n194:         weaponAimTarget.position = smoothed;\n195:     }\n196: \n197:     // --------------------------------------------------\n198:     // FIRE CONTROL\n199:     // --------------------------------------------------\n200: \n201:     private void TryFire(Vector3 targetPosition)\n202:     {\n203:         if (Time.time < nextFireTime)\n204:         {\n205:             return;\n206:         }          \n207: \n208:         if (isBursting == true && currentBurstShots >= burstCount)\n209:         {\n210:             isBursting = false;\n211:             currentBurstShots = 0;\n212:             burstCount = Random.Range(burstCountMin, burstCountMax);\n213:             nextBurstTime = Time.time + Random.Range(burstDelayMin, burstDelayMax);\n214:         }\n215: \n216:         if (automatic)\n217:         {\n218:             FireShot(targetPosition);\n219:             // SET NEXT FIRE TIME\n220:             nextFireTime = Time.time + fireRate;\n221:         }\n222:         else\n223:         {\n224:             if (!isBursting && Time.time > nextBurstTime)\n225:             {\n226:                 isBursting = true;\n227:             }\n228: \n229:             if (isBursting)\n230:             {\n231:                 if (currentBurstShots < burstCount)\n232:                 {\n233:                     FireShot(targetPosition);\n234: \n235:                     currentBurstShots++;\n236:                     // SET NEXT FIRE TIME\n237:                     nextFireTime = Time.time + fireRate;\n238:                 }\n239:             }\n240:         }\n241: \n242:         \n243:     }\n244: \n245:     // --------------------------------------------------\n246:     // SHOOTING\n247:     // --------------------------------------------------\n248: \n249:     private void FireShot(Vector3 targetPosition)\n250:     {\n251:         Vector3 shotDirection = GetSpreadDirection(targetPosition);\n252: \n253:         // RAYCAST HITSCAN\n254:         //This is okay for getting things setup, but I want to use a physical projectile instead of a raycast system\n255:         //TODO: Redo this section and instantiate a prefab bullet\n256:         if (Physics.Raycast(weaponMuzzle.position, shotDirection, out RaycastHit hit, attackRange))\n257:         {\n258:             AIHealth health = hit.collider.GetComponentInParent<AIHealth>();\n259: \n260:             if (health != null)\n261:             {\n262:                 if (health.entity.team != entity.team)\n263:                 {\n264:                     health.TakeDamage(20f);\n265:                 }\n266:             }\n267: \n268:             if (debugDrawShots)\n269:             {\n270:                 Debug.DrawLine(weaponMuzzle.position, hit.point, Color.red, 1f);\n271:             }\n272:         }\n273:         else\n274:         {\n275:             if (debugDrawShots)\n276:             {\n277:                 Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n278:             }\n279:         }\n280: \n281:         weaponMuzzle.GetComponent<VisualEffect>().Play();\n282:         weaponMuzzle.GetComponent<Light>().enabled = true;\n283:         lightOffTime = Time.time + muzzleLightDuration;\n284: \n285:         // PROCEDURAL RECOIL\n286:         if (weaponMotion != null)\n287:         {\n288:             weaponMotion.Fire(1f);\n289:         }\n290:     }\n291: \n292:     // --------------------------------------------------\n293:     // SPREAD\n294:     // --------------------------------------------------\n295: \n296:     private Vector3 GetSpreadDirection(Vector3 targetPosition)\n297:     {\n298:         Vector3 direction = (targetPosition - weaponMuzzle.position).normalized;\n299: \n300:         float finalSpread = spreadRadius;\n301: \n302:         // Moving inaccuracy\n303:         if (entity.movement != null)\n304:         {\n305:             float speed = entity.movement.GetVelocityMagnitude();\n306: \n307:             if (speed > 0.1f)\n308:             {\n309:                 finalSpread *= movingSpreadMultiplier;\n310:             }\n311:         }\n312: \n313:         Vector3 spreadOffset = Random.insideUnitSphere * finalSpread;\n314: \n315:         direction += spreadOffset;\n316: \n317:         return direction.normalized;\n318:     }\n319: \n320:     public void OverideSuppression()\n321:     {\n322:         isSuppressing = true;\n323: \n324:         endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n325:     }\n326: }"
```

### Assistant
Now I understand the current implementation. The `FireShot()` function currently uses raycast hit-scan shooting. The user wants to:

1. Replace the raycast with spawning a physical projectile prefab called "bullet"
2. Keep the spread feature (which is already implemented in `GetSpreadDirection()`)
3. The muzzle gameobject cannot be rotated (so we need to handle the projectile's rotation differently)

Since the muzzle cannot be rotated, we need to:
1. Instantiate the bullet prefab at the muzzle position
2. Calculate the spread direction using the existing `GetSpreadDirection()` method
3. Set the bullet's velocity/rotation based on that spread direction (not by rotating the muzzle)

I should provide an updated `FireShot()` function that:
- Uses `GameObject.Instantiate()` to spawn a bullet prefab
- Applies the spread direction to the bullet's velocity (assuming it has a Rigidbody)
- Keeps all the existing features like VFX, light, and recoil

Let me write the updated function. I'll need to add a reference to the bullet prefab as well.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "glob",
  "parameters": {
    "pattern": "**/bullet*.prefab",
    "path": "Assets"
  },
  "pluginIdentifier": "819843870"
}
```

### Tool
Tool call result:

```
"#error:filesystem_error\n#message:Filesystem operation failed\n#details:ripgrep exited with code 1"
```

### Assistant
The glob failed. Let me try searching for bullet-related files differently, or I can just provide the code assuming the prefab exists. The user mentioned a prefab "bullet" gameobject, so I'll assume it exists. Let me provide the updated FireShot() function.

Since the muzzle cannot be rotated, I need to:
1. Instantiate the bullet at the muzzle position
2. Calculate the spread direction using GetSpreadDirection()
3. Set the bullet's velocity/rotation based on that direction (not by rotating the muzzle)

I'll need to add a public reference to the bullet prefab in the script, and update the FireShot() function accordingly.

&lt;/think&gt;


Let me search for bullet-related files in the project:


Tool call request:

```json
{
  "name": "grep",
  "parameters": {
    "pattern": "bullet",
    "path": "Assets",
    "include": [
      "*.cs"
    ]
  },
  "pluginIdentifier": "960296809"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\n#pattern:bullet\n#offset:1\n#limit:6\n#total:6\n#has_more:false\n#matches_files:3\n#matches_0_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#matches_0_line:255\n#matches_0_content_bytes:65\n        //TODO: Redo this section and instantiate a prefab bullet\n#matches_1_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Utilities\\ScriptableObjects\\GunProfile.cs\n#matches_1_line:20\n#matches_1_content_bytes:35\n    public GameObject bulletPrefab;\n#matches_2_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Utilities\\ScriptableObjects\\GunProfile.cs\n#matches_2_line:21\n#matches_2_content_bytes:29\n    public float bulletSpeed;\n#matches_3_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Player\\TargetedGunFire.cs\n#matches_3_line:162\n#matches_3_content_bytes:125\n        GameObject bullet = Instantiate(gunProfile.bulletPrefab, muzzleObj.transform.position, muzzleObj.transform.rotation);\n#matches_4_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Player\\TargetedGunFire.cs\n#matches_4_line:163\n#matches_4_content_bytes:39\n        bullet.transform.parent = null;\n#matches_5_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Player\\TargetedGunFire.cs\n#matches_5_line:164\n#matches_5_content_bytes:153\n        bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - muzzleObj.transform.position).normalized * gunProfile.bulletSpeed;"
```

### Assistant
There's already a `GunProfile` scriptable object that has `bulletPrefab` and `bulletSpeed`. Let me check that file to understand the structure better, and also look at how `TargetedGunFire.cs` handles bullet instantiation to see the pattern used in this project.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "read",
  "parameters": {
    "filePath": "Assets/Utilities/ScriptableObjects/GunProfile.cs"
  },
  "pluginIdentifier": "172525313"
}
```
Tool call request:

```json
{
  "name": "read",
  "parameters": {
    "filePath": "Assets/Scripts/Player/TargetedGunFire.cs",
    "limit": 50,
    "offset": 155
  },
  "pluginIdentifier": "241231939"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Utilities\\ScriptableObjects\\GunProfile.cs\n#type:file\n#offset:1\n#limit:41\n#total:41\n#has_more:false\n#content_bytes:1330\n1: using System.Collections.Generic;\n2: using UnityEngine;\n3: using UnityEngine.VFX;\n4: \n5: [CreateAssetMenu(menuName = \"Scriptable Objects/Profiles/NewGunProfile\")]\n6: public class GunProfile : WeaponProfile\n7: {\n8:     [Header(\"Gun Details\")]\n9:     public List<ShootingModes> supportedModes;\n10: \n11:     public int magazineSize;\n12:     public float fireRate;\n13:     public float reloadTime;\n14: \n15:     public Vector3 muzzleLocalPosition;\n16:     public Vector3 muzzleLocalRotation;\n17:     public VisualEffectAsset muzzleEffectAsset;\n18:     public float muzzleLightDuration;\n19: \n20:     public GameObject bulletPrefab;\n21:     public float bulletSpeed;\n22: \n23:     [Header(\"Camera Recoil Settings\")]\n24:     public Vector2 cameraRecoilKick;\n25:     public float cameraReturnSpeed;\n26:     public float cameraSnappiness;\n27:     public float cameraRecoilScale;\n28:     public float adsCameraRecoilScale;\n29: \n30:     [Header(\"Weapon Recoil Settings\")]\n31:     public Vector3 weaponRecoilKick;\n32:     public float weaponReturnSpeed;\n33:     public float weaponSnappiness;\n34:     [Range(.001f, 1)]\n35:     public float minRecoilScale;\n36:     [Range(.001f, 1)]\n37:     public float maxRecoilScale;\n38:     public float adsAcumulationSpeed;\n39:     public float acumulationSpeed;\n40:     public float cooldownSpeed;\n41: }"
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Player\\TargetedGunFire.cs\n#type:file\n#offset:155\n#limit:16\n#total:170\n#has_more:false\n#content_bytes:672\n155:         }\n156:     }\n157: \n158:     void FireWeapon()\n159:     {\n160:         recoilController.ApplyRecoil();\n161: \n162:         GameObject bullet = Instantiate(gunProfile.bulletPrefab, muzzleObj.transform.position, muzzleObj.transform.rotation);\n163:         bullet.transform.parent = null;\n164:         bullet.GetComponent<Rigidbody>().linearVelocity = (target.transform.position - muzzleObj.transform.position).normalized * gunProfile.bulletSpeed;\n165: \n166:         muzzleObj.GetComponent<VisualEffect>().Play();\n167:         muzzleObj.GetComponent<Light>().enabled = true;\n168:         lightOffTime = Time.time + gunProfile.muzzleLightDuration;\n169:     }\n170: }"
```

### Assistant
Now I understand the pattern. The project uses a `GunProfile` scriptable object that contains `bulletPrefab` and `bulletSpeed`. The bullet has a Rigidbody component.

For the AICombat script, I need to:
1. Add a reference to a bullet prefab (either directly or through a GunProfile)
2. Update the `FireShot()` function to instantiate the bullet at the muzzle position
3. Apply the spread direction to the bullet's velocity instead of using raycast
4. Keep the muzzle unrotated (don't rotate it, just use the spread direction for the bullet)

Looking at the current AICombat script, it doesn't use GunProfile directly. I should add a bullet prefab reference and bullet speed to the AICombat script, or we could add a GunProfile reference. Let me check if AICombat already has access to weapon data...

Looking at the script, it has:
- `weaponMuzzle` Transform
- `weaponMotion` ProceduralWeaponMotion
- Fire settings like fireRate, spreadRadius, etc.

I think the cleanest approach is to add a `bulletPrefab` and `bulletSpeed` field directly to AICombat, similar to how GunProfile has them. This keeps the AI system self-contained.

Here's my updated FireShot() function:

```csharp
private void FireShot(Vector3 targetPosition)
{
    Vector3 shotDirection = GetSpreadDirection(targetPosition);
    
    // SPAWN PHYSICAL PROJECTILE
    GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);
    bullet.transform.parent = null;
    bullet.GetComponent&lt;Rigidbody&gt;().velocity = shotDirection * bulletSpeed;
    
    if (debugDrawShots)
    {
        Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);
    }
    
    weaponMuzzle.GetComponent&lt;VisualEffect&gt;().Play();
    weaponMuzzle.GetComponent&lt;Light&gt;().enabled = true;
    lightOffTime = Time.time + muzzleLightDuration;
    
    // PROCEDURAL RECOIL
    if (weaponMotion != null)
    {
        weaponMotion.Fire(1f);
    }
}
```

I need to add the required fields: `public GameObject bulletPrefab` and `public float bulletSpeed`. The key insight is that since the muzzle can't rotate, I'm using `Quaternion.identity` for the bullet's initial rotation and instead applying the spread direction directly to the Rigidbody's velocity. This handles the bullet spread without needing to rotate the muzzle itself.

For damage handling, the bullet prefab should manage its own collision detection rather than relying on raycast hit detection like the current system does.

&lt;/think&gt;


Now I understand the project structure. The bullet prefab has a Rigidbody component. Here's the updated `FireShot()` function that spawns a physical projectile with spread, without rotating the muzzle:

```csharp|CODE_EDIT_BLOCK|Assets/Scripts/Bots/AICombat.cs
// ... CODE_OMITTED ...

    [Header("Accuracy")]
    [Tooltip("Lower = more accurate")]
    public float spreadRadius = 0.15f;

    [Tooltip("Extra spread while moving")]
    public float movingSpreadMultiplier = 2f;

    [Header("Projectile")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 100f;

    [Header("Search Fire")]
// ... CODE_OMITTED ...

    // --------------------------------------------------
    // SHOOTING
    // --------------------------------------------------

    private void FireShot(Vector3 targetPosition)
    {
        Vector3 shotDirection = GetSpreadDirection(targetPosition);

        // SPAWN PHYSICAL PROJECTILE
        GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);
        bullet.transform.parent = null;
        bullet.GetComponent&lt;Rigidbody&gt;().velocity = shotDirection * bulletSpeed;

        if (debugDrawShots)
        {
            Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);
        }

        weaponMuzzle.GetComponent&lt;VisualEffect&gt;().Play();
        weaponMuzzle.GetComponent&lt;Light&gt;().enabled = true;
        lightOffTime = Time.time + muzzleLightDuration;

        // PROCEDURAL RECOIL
        if (weaponMotion != null)
        {
            weaponMotion.Fire(1f);
        }
    }

    // ... CODE_OMITTED ...
```

**Key changes:**

1. **Added `[Header("Projectile")]` fields** – `bulletPrefab` reference and `bulletSpeed` for configuring the projectile in the Inspector.

2. **Replaced raycast with `Instantiate`** – The bullet spawns at `weaponMuzzle.position` with `Quaternion.identity` rotation (muzzle stays unrotated).

3. **Applied spread via velocity** – The `shotDirection` (which already includes spread from `GetSpreadDirection()`) is multiplied by `bulletSpeed` and assigned directly to the Rigidbody's velocity.

4. **Removed raycast damage logic** – The bullet prefab itself should handle damage on collision (likely via a script on the bullet that checks for `AIHealth` components).

5. **Changed debug draw** – Only draws the ray (no hit point to draw to since we're not raycasting anymore).

**Note:** Make sure your bullet prefab has a script that handles damage on collision, since the raycast-based `TakeDamage()` call is no longer in this function.

### User
This is great. I have implemented it. moving on to the next thing. Can you read the current AICombat file, and make a change to reset the `weaponAimTarget` position back to its starting location when the bot is out of combat? this keeps the bot looking forward when on patrol or searching.

### Assistant
The user wants me to read the current AICombat.cs file again (since they've implemented my previous changes) and add functionality to reset the `weaponAimTarget` position when the bot is out of combat (i.e., when not in Combat or Suppress states). This would make the bot look forward during patrol or searching states.

Let me read the current file first to see its current state.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "read",
  "parameters": {
    "filePath": "Assets/Scripts/Bots/AICombat.cs"
  },
  "pluginIdentifier": "404238393"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#type:file\n#offset:1\n#limit:354\n#total:354\n#has_more:false\n#content_bytes:10976\n1: using Unity.VisualScripting;\n2: using UnityEngine;\n3: using UnityEngine.VFX;\n4: \n5: /// <summary>\n6: /// Handles:\n7: /// - Target aiming\n8: /// - Fire timing\n9: /// - Burst fire\n10: /// - Accuracy/spread\n11: /// - Weapon integration\n12: /// - Procedural recoil\n13: /// \n14: /// Does NOT:\n15: /// - Control movement\n16: /// - Decide AI states\n17: /// - Handle perception\n18: /// </summary>\n19: public class AICombat : MonoBehaviour, IAIBehaviour\n20: {\n21:     public AIEntity entity;\n22: \n23:     [Header(\"References\")]\n24:     public Transform weaponMuzzle;\n25: \n26:     [Tooltip(\"Transform the rig aims toward\")]\n27:     public Transform weaponAimTarget;\n28: \n29:     public ProceduralWeaponMotion weaponMotion;\n30: \n31:     [Header(\"Combat\")]\n32:     public float attackRange = 25f;\n33: \n34:     [Tooltip(\"How quickly AI rotates aim target\")]\n35:     public float aimSpeed = 12f;\n36: \n37:     [Header(\"Fire Settings\")]\n38:     public bool automatic = true;\n39: \n40:     public float fireRate = 0.1f;\n41: \n42:     [Space]\n43:     public int burstCountMin = 2;\n44:     public int burstCountMax = 5;\n45:     [Space]\n46:     public float burstDelayMin = 0.08f;\n47:     public float burstDelayMax = 0.25f;\n48: \n49:     [Header(\"Accuracy\")]\n50:     [Tooltip(\"Lower = more accurate\")]\n51:     public float spreadRadius = 0.15f;\n52: \n53:     [Tooltip(\"Extra spread while moving\")]\n54:     public float movingSpreadMultiplier = 2f;\n55: \n56:     [Header(\"Projectile\")]\n57:     public GameObject bulletPrefab;\n58:     public float bulletSpeed = 100f;\n59: \n60:     [Header(\"Search Fire\")]\n61:     public bool suppressLastKnownPosition = true;\n62:     public float suppressionDurationMin;\n63:     public float suppressionDurationMax;\n64:     private float endSuppressionTime;\n65: \n66:     [Tooltip(\"Chance to continue firing at last known position\")]\n67:     [Range(0f, 1f)]\n68:     public float suppressionFireChance = 0.5f;\n69: \n70:     [Header(\"Debug\")]\n71:     public bool debugDrawShots = true;\n72: \n73:     public float nextFireTime;\n74:     public float nextBurstTime;\n75: \n76:     public int burstCount;\n77:     public int currentBurstShots;\n78:     public bool isBursting;\n79: \n80:     public bool isSuppressing;\n81: \n82:     private float lightOffTime = 0f;\n83:     private float muzzleLightDuration = .1f;\n84: \n85:     public void Initialize(AIEntity entity)\n86:     {\n87:         this.entity = entity;\n88: \n89:         burstCount = Random.Range(burstCountMin, burstCountMax);\n90:     }\n91: \n92:     private void Update()\n93:     {\n94:         if (entity.currentState == AIState.Dead)\n95:         {\n96:             return;\n97:         }\n98: \n99:         switch (entity.currentState)\n100:         {\n101:             case AIState.Combat:\n102:                 HandleCombat();\n103:                 break;\n104: \n105:             case AIState.Suppress:\n106:                 if (isSuppressing == false)\n107:                 {\n108:                     isSuppressing = true;\n109:                     endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n110:                 }\n111:                 \n112:                 if (isSuppressing == true)\n113:                 {\n114:                     if (Time.time >= endSuppressionTime)\n115:                     {\n116:                         isSuppressing = false;\n117:                     }\n118:                     else\n119:                     {\n120:                         HandleSuppressionFire();\n121:                     }\n122:                 }\n123:                 break;\n124:         }\n125: \n126:         if (weaponMuzzle.GetComponent<Light>().enabled)\n127:         {\n128:             if (Time.time >= lightOffTime)\n129:             {\n130:                 weaponMuzzle.GetComponent<Light>().enabled = false;\n131:             }\n132:         }\n133:     }\n134: \n135:     // --------------------------------------------------\n136:     // COMBAT\n137:     // --------------------------------------------------\n138: \n139:     private void HandleCombat()\n140:     {\n141:         Transform target = entity.perception.currentTarget;\n142: \n143:         if (target == null)\n144:         {\n145:             return;\n146:         }\n147: \n148:         float distance = Vector3.Distance(transform.position, target.position);\n149: \n150:         UpdateAim(target.position);\n151: \n152:         if (distance <= attackRange)\n153:         {\n154:             TryFire(target.position);\n155:         }\n156:     }\n157: \n158:     // --------------------------------------------------\n159:     // SUPPRESSION FIRE\n160:     // --------------------------------------------------\n161: \n162:     private void HandleSuppressionFire()\n163:     {\n164:         if (!suppressLastKnownPosition)\n165:         {\n166:             return;\n167:         }\n168: \n169:         if (Random.value > suppressionFireChance)\n170:         {\n171:             return;\n172:         }\n173: \n174:         UpdateAim(\n175:             entity.perception.lastKnownPosition\n176:         );\n177: \n178:         TryFire(\n179:             entity.perception.lastKnownPosition\n180:         );\n181:     }\n182: \n183:     // --------------------------------------------------\n184:     // AIMING\n185:     // --------------------------------------------------\n186: \n187:     private void UpdateAim(Vector3 targetPosition)\n188:     {\n189:         if (weaponAimTarget == null)\n190:         {\n191:             return;\n192:         }\n193: \n194:         Vector3 current = weaponAimTarget.position;\n195: \n196:         Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);\n197: \n198:         weaponAimTarget.position = smoothed;\n199:     }\n200: \n201:     // --------------------------------------------------\n202:     // FIRE CONTROL\n203:     // --------------------------------------------------\n204: \n205:     private void TryFire(Vector3 targetPosition)\n206:     {\n207:         if (Time.time < nextFireTime)\n208:         {\n209:             return;\n210:         }          \n211: \n212:         if (isBursting == true && currentBurstShots >= burstCount)\n213:         {\n214:             isBursting = false;\n215:             currentBurstShots = 0;\n216:             burstCount = Random.Range(burstCountMin, burstCountMax);\n217:             nextBurstTime = Time.time + Random.Range(burstDelayMin, burstDelayMax);\n218:         }\n219: \n220:         if (automatic)\n221:         {\n222:             FireShot(targetPosition);\n223:             // SET NEXT FIRE TIME\n224:             nextFireTime = Time.time + fireRate;\n225:         }\n226:         else\n227:         {\n228:             if (!isBursting && Time.time > nextBurstTime)\n229:             {\n230:                 isBursting = true;\n231:             }\n232: \n233:             if (isBursting)\n234:             {\n235:                 if (currentBurstShots < burstCount)\n236:                 {\n237:                     FireShot(targetPosition);\n238: \n239:                     currentBurstShots++;\n240:                     // SET NEXT FIRE TIME\n241:                     nextFireTime = Time.time + fireRate;\n242:                 }\n243:             }\n244:         }\n245: \n246:         \n247:     }\n248: \n249:     // --------------------------------------------------\n250:     // SHOOTING\n251:     // --------------------------------------------------\n252:     private void FireShot(Vector3 targetPosition)\n253:     {\n254:         Vector3 shotDirection = GetSpreadDirection(targetPosition);\n255: \n256:         // SPAWN PHYSICAL PROJECTILE\n257:         GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);\n258:         bullet.transform.parent = null;\n259:         bullet.GetComponent<Rigidbody>().linearVelocity = shotDirection * bulletSpeed;\n260: \n261:         if (debugDrawShots)\n262:         {\n263:             Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n264:         }\n265: \n266:         weaponMuzzle.GetComponent<VisualEffect>().Play();\n267:         weaponMuzzle.GetComponent<Light>().enabled = true;\n268:         lightOffTime = Time.time + muzzleLightDuration;\n269: \n270:         // PROCEDURAL RECOIL\n271:         if (weaponMotion != null)\n272:         {\n273:             weaponMotion.Fire(1f);\n274:         }\n275:     }\n276: \n277:     private void FireRaycastShot(Vector3 targetPosition)\n278:     {\n279:         Vector3 shotDirection = GetSpreadDirection(targetPosition);\n280: \n281:         // RAYCAST HITSCAN\n282:         //This is okay for getting things setup, but I want to use a physical projectile instead of a raycast system\n283:         //TODO: Redo this section and instantiate a prefab bullet\n284:         if (Physics.Raycast(weaponMuzzle.position, shotDirection, out RaycastHit hit, attackRange))\n285:         {\n286:             AIHealth health = hit.collider.GetComponentInParent<AIHealth>();\n287: \n288:             if (health != null)\n289:             {\n290:                 if (health.entity.team != entity.team)\n291:                 {\n292:                     health.TakeDamage(20f);\n293:                 }\n294:             }\n295: \n296:             if (debugDrawShots)\n297:             {\n298:                 Debug.DrawLine(weaponMuzzle.position, hit.point, Color.red, 1f);\n299:             }\n300:         }\n301:         else\n302:         {\n303:             if (debugDrawShots)\n304:             {\n305:                 Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n306:             }\n307:         }\n308: \n309:         weaponMuzzle.GetComponent<VisualEffect>().Play();\n310:         weaponMuzzle.GetComponent<Light>().enabled = true;\n311:         lightOffTime = Time.time + muzzleLightDuration;\n312: \n313:         // PROCEDURAL RECOIL\n314:         if (weaponMotion != null)\n315:         {\n316:             weaponMotion.Fire(1f);\n317:         }\n318:     }\n319: \n320:     // --------------------------------------------------\n321:     // SPREAD\n322:     // --------------------------------------------------\n323: \n324:     private Vector3 GetSpreadDirection(Vector3 targetPosition)\n325:     {\n326:         Vector3 direction = (targetPosition - weaponMuzzle.position).normalized;\n327: \n328:         float finalSpread = spreadRadius;\n329: \n330:         // Moving inaccuracy\n331:         if (entity.movement != null)\n332:         {\n333:             float speed = entity.movement.GetVelocityMagnitude();\n334: \n335:             if (speed > 0.1f)\n336:             {\n337:                 finalSpread *= movingSpreadMultiplier;\n338:             }\n339:         }\n340: \n341:         Vector3 spreadOffset = Random.insideUnitSphere * finalSpread;\n342: \n343:         direction += spreadOffset;\n344: \n345:         return direction.normalized;\n346:     }\n347: \n348:     public void OverideSuppression()\n349:     {\n350:         isSuppressing = true;\n351: \n352:         endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n353:     }\n354: }"
```

### Assistant
Looking at the code, I need to:

1. Store the starting position of `weaponAimTarget` (probably in `Initialize` or as a cached field)
2. Add logic in `Update()` to reset the aim target when not in Combat or Suppress states

Let me check what AI states exist by looking for the AIState enum or how states are used. I should also understand what "out of combat" means - likely Patrol, Search, Idle, etc.

The switch statement in Update only handles `Combat` and `Suppress` states. I need to add an `else` or `default` case that resets the aim target back to its starting position.

I'll need to:
1. Add a field to store the starting/rest position of the weaponAimTarget
2. Cache it in Initialize
3. Add a default/else case in the Update switch to smoothly return to the rest position

Let me also check if there's a way to know what states exist.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "grep",
  "parameters": {
    "include": [
      "*.cs"
    ],
    "path": "Assets/Scripts/Bots",
    "pattern": "enum AIState|AIState\\."
  },
  "pluginIdentifier": "101997321"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\n#pattern:enum AIState|AIState\\.\n#offset:1\n#limit:37\n#total:37\n#has_more:false\n#matches_files:5\n#matches_0_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#matches_0_line:94\n#matches_0_content_bytes:48\n        if (entity.currentState == AIState.Dead)\n#matches_1_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#matches_1_line:101\n#matches_1_content_bytes:32\n            case AIState.Combat:\n#matches_2_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#matches_2_line:105\n#matches_2_content_bytes:34\n            case AIState.Suppress:\n#matches_3_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIPatrol.cs\n#matches_3_line:42\n#matches_3_content_bytes:50\n        if (entity.currentState == AIState.Combat)\n#matches_4_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_4_line:212\n#matches_4_content_bytes:48\n                member.SetState(AIState.Combat);\n#matches_5_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_5_line:253\n#matches_5_content_bytes:50\n                member.SetState(AIState.Suppress);\n#matches_6_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_6_line:268\n#matches_6_content_bytes:48\n                member.SetState(AIState.Search);\n#matches_7_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_7_line:290\n#matches_7_content_bytes:64\n            if (searcher.currentState == AIState.Dead) continue;\n#matches_8_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_8_line:307\n#matches_8_content_bytes:66\n                if (member.currentState == AIState.Dead) continue;\n#matches_9_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_9_line:321\n#matches_9_content_bytes:48\n                member.SetState(AIState.Patrol);\n#matches_10_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_10_line:338\n#matches_10_content_bytes:62\n            if (member.currentState == AIState.Dead) continue;\n#matches_11_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_11_line:361\n#matches_11_content_bytes:62\n            if (member.currentState == AIState.Dead) continue;\n#matches_12_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_12_line:364\n#matches_12_content_bytes:54\n            if (member.currentState == AIState.Combat)\n#matches_13_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_13_line:380\n#matches_13_content_bytes:99\n                if (member.currentState != AIState.Patrol && member.currentState != AIState.Combat)\n#matches_14_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AISquad.cs\n#matches_14_line:382\n#matches_14_content_bytes:52\n                    member.SetState(AIState.Patrol);\n#matches_15_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_15_line:9\n#matches_15_content_bytes:47\n    public AIState currentState = AIState.Idle;\n#matches_16_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_16_line:51\n#matches_16_content_bytes:66\n        if (currentState == AIState.Dead || team == AITeam.Player)\n#matches_17_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_17_line:87\n#matches_17_content_bytes:30\n            case AIState.Idle:\n#matches_18_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_18_line:90\n#matches_18_content_bytes:45\n                    SetState(AIState.Combat);\n#matches_19_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_19_line:98\n#matches_19_content_bytes:49\n                        SetState(AIState.Search);\n#matches_20_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_20_line:102\n#matches_20_content_bytes:32\n            case AIState.Patrol:\n#matches_21_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_21_line:106\n#matches_21_content_bytes:45\n                    SetState(AIState.Combat);\n#matches_22_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_22_line:112\n#matches_22_content_bytes:45\n                    SetState(AIState.Search);\n#matches_23_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_23_line:116\n#matches_23_content_bytes:32\n            case AIState.Combat:\n#matches_24_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_24_line:120\n#matches_24_content_bytes:47\n                    SetState(AIState.Suppress);\n#matches_25_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_25_line:124\n#matches_25_content_bytes:34\n            case AIState.Suppress:\n#matches_26_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_26_line:128\n#matches_26_content_bytes:45\n                    SetState(AIState.Combat);\n#matches_27_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_27_line:136\n#matches_27_content_bytes:49\n                        SetState(AIState.Search);\n#matches_28_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_28_line:141\n#matches_28_content_bytes:32\n            case AIState.Search:\n#matches_29_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_29_line:145\n#matches_29_content_bytes:45\n                    SetState(AIState.Combat);\n#matches_30_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_30_line:150\n#matches_30_content_bytes:43\n                    SetState(AIState.Idle);\n#matches_31_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIEntity.cs\n#matches_31_line:163\n#matches_31_content_bytes:36\n        currentState = AIState.Dead;\n#matches_32_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIMovement.cs\n#matches_32_line:44\n#matches_32_content_bytes:48\n        if (entity.currentState == AIState.Dead)\n#matches_33_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIMovement.cs\n#matches_33_line:53\n#matches_33_content_bytes:30\n            case AIState.Idle:\n#matches_34_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIMovement.cs\n#matches_34_line:58\n#matches_34_content_bytes:32\n            case AIState.Patrol:\n#matches_35_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIMovement.cs\n#matches_35_line:66\n#matches_35_content_bytes:32\n            case AIState.Combat:\n#matches_36_path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AIMovement.cs\n#matches_36_line:81\n#matches_36_content_bytes:32\n            case AIState.Search:"
```

### Assistant
Now I can see the AI states: Idle, Patrol, Combat, Suppress, Search, and Dead.

The user wants the weaponAimTarget to reset to its starting position when the bot is NOT in Combat or Suppress states (i.e., when in Patrol, Search, or Idle states).

I need to:
1. Store the starting/rest position of weaponAimTarget (cache it during Initialize)
2. Add a default/else case in the Update switch that smoothly returns the aim target to the rest position when not in Combat or Suppress

Let me provide the updated code.

&lt;/think&gt;


Now I can see the AI states: `Idle`, `Patrol`, `Combat`, `Suppress`, `Search`, and `Dead`. I'll add a rest position that gets cached on initialization and smoothly returned to when not in combat states.

```csharp|CODE_EDIT_BLOCK|Assets/Scripts/Bots/AICombat.cs
// ... CODE_OMITTED ...

    private float lightOffTime = 0f;
    private float muzzleLightDuration = .1f;

    private Vector3 aimTargetRestPosition;

    public void Initialize(AIEntity entity)
    {
        this.entity = entity;

        burstCount = Random.Range(burstCountMin, burstCountMax);

        if (weaponAimTarget != null)
        {
            aimTargetRestPosition = weaponAimTarget.position;
        }
    }

    private void Update()
    {
        if (entity.currentState == AIState.Dead)
        {
            return;
        }

        switch (entity.currentState)
        {
            case AIState.Combat:
                HandleCombat();
                break;

            case AIState.Suppress:
                if (isSuppressing == false)
                {
                    isSuppressing = true;
                    endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);
                }
                
                if (isSuppressing == true)
                {
                    if (Time.time &gt;= endSuppressionTime)
                    {
                        isSuppressing = false;
                    }
                    else
                    {
                        HandleSuppressionFire();
                    }
                }
                break;

            default:
                ResetAimToRest();
                break;
        }

        if (weaponMuzzle.GetComponent&lt;Light&gt;().enabled)
        {
            if (Time.time &gt;= lightOffTime)
            {
                weaponMuzzle.GetComponent&lt;Light&gt;().enabled = false;
            }
        }
    }

    // ... CODE_OMITTED ...

    // --------------------------------------------------
    // AIMING
    // --------------------------------------------------

    private void UpdateAim(Vector3 targetPosition)
    {
        if (weaponAimTarget == null)
        {
            return;
        }

        Vector3 current = weaponAimTarget.position;

        Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);

        weaponAimTarget.position = smoothed;
    }

    private void ResetAimToRest()
    {
        if (weaponAimTarget == null)
        {
            return;
        }

        Vector3 current = weaponAimTarget.position;
        Vector3 smoothed = Vector3.Lerp(current, aimTargetRestPosition, Time.deltaTime * aimSpeed);

        weaponAimTarget.position = smoothed;
    }

    // ... CODE_OMITTED ...
```

**Key changes:**

1. **Added `aimTargetRestPosition` field** – Stores the starting/rest position of the aim target.

2. **Cached rest position in `Initialize()`** – Captures the weaponAimTarget's position when the bot is first set up.

3. **Added `default` case in the switch** – Calls `ResetAimToRest()` when the bot is in any state other than `Combat` or `Suppress` (i.e., `Idle`, `Patrol`, or `Search`).

4. **Created `ResetAimToRest()` method** – Smoothly lerps the aim target back to its rest position using the same `aimSpeed` for consistent feel.

The bot will now smoothly return its weapon aim to forward when transitioning out of combat states.

### User
Can you update the file with these changes?

### Assistant
The user wants me to apply the changes to the actual file. I need to use the create tool with overwrite to update the AICombat.cs file with all the changes we've discussed:

1. The projectile spawning (already done by user)
2. The aim target reset functionality (new)

Let me read the current file again to make sure I have the latest version, then apply all changes.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "read",
  "parameters": {
    "filePath": "Assets/Scripts/Bots/AICombat.cs"
  },
  "pluginIdentifier": "214884584"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#type:file\n#offset:1\n#limit:354\n#total:354\n#has_more:false\n#content_bytes:10976\n1: using Unity.VisualScripting;\n2: using UnityEngine;\n3: using UnityEngine.VFX;\n4: \n5: /// <summary>\n6: /// Handles:\n7: /// - Target aiming\n8: /// - Fire timing\n9: /// - Burst fire\n10: /// - Accuracy/spread\n11: /// - Weapon integration\n12: /// - Procedural recoil\n13: /// \n14: /// Does NOT:\n15: /// - Control movement\n16: /// - Decide AI states\n17: /// - Handle perception\n18: /// </summary>\n19: public class AICombat : MonoBehaviour, IAIBehaviour\n20: {\n21:     public AIEntity entity;\n22: \n23:     [Header(\"References\")]\n24:     public Transform weaponMuzzle;\n25: \n26:     [Tooltip(\"Transform the rig aims toward\")]\n27:     public Transform weaponAimTarget;\n28: \n29:     public ProceduralWeaponMotion weaponMotion;\n30: \n31:     [Header(\"Combat\")]\n32:     public float attackRange = 25f;\n33: \n34:     [Tooltip(\"How quickly AI rotates aim target\")]\n35:     public float aimSpeed = 12f;\n36: \n37:     [Header(\"Fire Settings\")]\n38:     public bool automatic = true;\n39: \n40:     public float fireRate = 0.1f;\n41: \n42:     [Space]\n43:     public int burstCountMin = 2;\n44:     public int burstCountMax = 5;\n45:     [Space]\n46:     public float burstDelayMin = 0.08f;\n47:     public float burstDelayMax = 0.25f;\n48: \n49:     [Header(\"Accuracy\")]\n50:     [Tooltip(\"Lower = more accurate\")]\n51:     public float spreadRadius = 0.15f;\n52: \n53:     [Tooltip(\"Extra spread while moving\")]\n54:     public float movingSpreadMultiplier = 2f;\n55: \n56:     [Header(\"Projectile\")]\n57:     public GameObject bulletPrefab;\n58:     public float bulletSpeed = 100f;\n59: \n60:     [Header(\"Search Fire\")]\n61:     public bool suppressLastKnownPosition = true;\n62:     public float suppressionDurationMin;\n63:     public float suppressionDurationMax;\n64:     private float endSuppressionTime;\n65: \n66:     [Tooltip(\"Chance to continue firing at last known position\")]\n67:     [Range(0f, 1f)]\n68:     public float suppressionFireChance = 0.5f;\n69: \n70:     [Header(\"Debug\")]\n71:     public bool debugDrawShots = true;\n72: \n73:     public float nextFireTime;\n74:     public float nextBurstTime;\n75: \n76:     public int burstCount;\n77:     public int currentBurstShots;\n78:     public bool isBursting;\n79: \n80:     public bool isSuppressing;\n81: \n82:     private float lightOffTime = 0f;\n83:     private float muzzleLightDuration = .1f;\n84: \n85:     public void Initialize(AIEntity entity)\n86:     {\n87:         this.entity = entity;\n88: \n89:         burstCount = Random.Range(burstCountMin, burstCountMax);\n90:     }\n91: \n92:     private void Update()\n93:     {\n94:         if (entity.currentState == AIState.Dead)\n95:         {\n96:             return;\n97:         }\n98: \n99:         switch (entity.currentState)\n100:         {\n101:             case AIState.Combat:\n102:                 HandleCombat();\n103:                 break;\n104: \n105:             case AIState.Suppress:\n106:                 if (isSuppressing == false)\n107:                 {\n108:                     isSuppressing = true;\n109:                     endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n110:                 }\n111:                 \n112:                 if (isSuppressing == true)\n113:                 {\n114:                     if (Time.time >= endSuppressionTime)\n115:                     {\n116:                         isSuppressing = false;\n117:                     }\n118:                     else\n119:                     {\n120:                         HandleSuppressionFire();\n121:                     }\n122:                 }\n123:                 break;\n124:         }\n125: \n126:         if (weaponMuzzle.GetComponent<Light>().enabled)\n127:         {\n128:             if (Time.time >= lightOffTime)\n129:             {\n130:                 weaponMuzzle.GetComponent<Light>().enabled = false;\n131:             }\n132:         }\n133:     }\n134: \n135:     // --------------------------------------------------\n136:     // COMBAT\n137:     // --------------------------------------------------\n138: \n139:     private void HandleCombat()\n140:     {\n141:         Transform target = entity.perception.currentTarget;\n142: \n143:         if (target == null)\n144:         {\n145:             return;\n146:         }\n147: \n148:         float distance = Vector3.Distance(transform.position, target.position);\n149: \n150:         UpdateAim(target.position);\n151: \n152:         if (distance <= attackRange)\n153:         {\n154:             TryFire(target.position);\n155:         }\n156:     }\n157: \n158:     // --------------------------------------------------\n159:     // SUPPRESSION FIRE\n160:     // --------------------------------------------------\n161: \n162:     private void HandleSuppressionFire()\n163:     {\n164:         if (!suppressLastKnownPosition)\n165:         {\n166:             return;\n167:         }\n168: \n169:         if (Random.value > suppressionFireChance)\n170:         {\n171:             return;\n172:         }\n173: \n174:         UpdateAim(\n175:             entity.perception.lastKnownPosition\n176:         );\n177: \n178:         TryFire(\n179:             entity.perception.lastKnownPosition\n180:         );\n181:     }\n182: \n183:     // --------------------------------------------------\n184:     // AIMING\n185:     // --------------------------------------------------\n186: \n187:     private void UpdateAim(Vector3 targetPosition)\n188:     {\n189:         if (weaponAimTarget == null)\n190:         {\n191:             return;\n192:         }\n193: \n194:         Vector3 current = weaponAimTarget.position;\n195: \n196:         Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);\n197: \n198:         weaponAimTarget.position = smoothed;\n199:     }\n200: \n201:     // --------------------------------------------------\n202:     // FIRE CONTROL\n203:     // --------------------------------------------------\n204: \n205:     private void TryFire(Vector3 targetPosition)\n206:     {\n207:         if (Time.time < nextFireTime)\n208:         {\n209:             return;\n210:         }          \n211: \n212:         if (isBursting == true && currentBurstShots >= burstCount)\n213:         {\n214:             isBursting = false;\n215:             currentBurstShots = 0;\n216:             burstCount = Random.Range(burstCountMin, burstCountMax);\n217:             nextBurstTime = Time.time + Random.Range(burstDelayMin, burstDelayMax);\n218:         }\n219: \n220:         if (automatic)\n221:         {\n222:             FireShot(targetPosition);\n223:             // SET NEXT FIRE TIME\n224:             nextFireTime = Time.time + fireRate;\n225:         }\n226:         else\n227:         {\n228:             if (!isBursting && Time.time > nextBurstTime)\n229:             {\n230:                 isBursting = true;\n231:             }\n232: \n233:             if (isBursting)\n234:             {\n235:                 if (currentBurstShots < burstCount)\n236:                 {\n237:                     FireShot(targetPosition);\n238: \n239:                     currentBurstShots++;\n240:                     // SET NEXT FIRE TIME\n241:                     nextFireTime = Time.time + fireRate;\n242:                 }\n243:             }\n244:         }\n245: \n246:         \n247:     }\n248: \n249:     // --------------------------------------------------\n250:     // SHOOTING\n251:     // --------------------------------------------------\n252:     private void FireShot(Vector3 targetPosition)\n253:     {\n254:         Vector3 shotDirection = GetSpreadDirection(targetPosition);\n255: \n256:         // SPAWN PHYSICAL PROJECTILE\n257:         GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);\n258:         bullet.transform.parent = null;\n259:         bullet.GetComponent<Rigidbody>().linearVelocity = shotDirection * bulletSpeed;\n260: \n261:         if (debugDrawShots)\n262:         {\n263:             Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n264:         }\n265: \n266:         weaponMuzzle.GetComponent<VisualEffect>().Play();\n267:         weaponMuzzle.GetComponent<Light>().enabled = true;\n268:         lightOffTime = Time.time + muzzleLightDuration;\n269: \n270:         // PROCEDURAL RECOIL\n271:         if (weaponMotion != null)\n272:         {\n273:             weaponMotion.Fire(1f);\n274:         }\n275:     }\n276: \n277:     private void FireRaycastShot(Vector3 targetPosition)\n278:     {\n279:         Vector3 shotDirection = GetSpreadDirection(targetPosition);\n280: \n281:         // RAYCAST HITSCAN\n282:         //This is okay for getting things setup, but I want to use a physical projectile instead of a raycast system\n283:         //TODO: Redo this section and instantiate a prefab bullet\n284:         if (Physics.Raycast(weaponMuzzle.position, shotDirection, out RaycastHit hit, attackRange))\n285:         {\n286:             AIHealth health = hit.collider.GetComponentInParent<AIHealth>();\n287: \n288:             if (health != null)\n289:             {\n290:                 if (health.entity.team != entity.team)\n291:                 {\n292:                     health.TakeDamage(20f);\n293:                 }\n294:             }\n295: \n296:             if (debugDrawShots)\n297:             {\n298:                 Debug.DrawLine(weaponMuzzle.position, hit.point, Color.red, 1f);\n299:             }\n300:         }\n301:         else\n302:         {\n303:             if (debugDrawShots)\n304:             {\n305:                 Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n306:             }\n307:         }\n308: \n309:         weaponMuzzle.GetComponent<VisualEffect>().Play();\n310:         weaponMuzzle.GetComponent<Light>().enabled = true;\n311:         lightOffTime = Time.time + muzzleLightDuration;\n312: \n313:         // PROCEDURAL RECOIL\n314:         if (weaponMotion != null)\n315:         {\n316:             weaponMotion.Fire(1f);\n317:         }\n318:     }\n319: \n320:     // --------------------------------------------------\n321:     // SPREAD\n322:     // --------------------------------------------------\n323: \n324:     private Vector3 GetSpreadDirection(Vector3 targetPosition)\n325:     {\n326:         Vector3 direction = (targetPosition - weaponMuzzle.position).normalized;\n327: \n328:         float finalSpread = spreadRadius;\n329: \n330:         // Moving inaccuracy\n331:         if (entity.movement != null)\n332:         {\n333:             float speed = entity.movement.GetVelocityMagnitude();\n334: \n335:             if (speed > 0.1f)\n336:             {\n337:                 finalSpread *= movingSpreadMultiplier;\n338:             }\n339:         }\n340: \n341:         Vector3 spreadOffset = Random.insideUnitSphere * finalSpread;\n342: \n343:         direction += spreadOffset;\n344: \n345:         return direction.normalized;\n346:     }\n347: \n348:     public void OverideSuppression()\n349:     {\n350:         isSuppressing = true;\n351: \n352:         endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n353:     }\n354: }"
```

### Assistant
The user wants me to update the file with the changes. I need to:
1. Add the `aimTargetRestPosition` field
2. Update `Initialize()` to cache the rest position
3. Add the `default` case in the Update switch
4. Add the `ResetAimToRest()` method

Let me write the complete updated file.

&lt;/think&gt;
Tool call request:

```json
{
  "name": "create",
  "parameters": {
    "type": "file",
    "path": "Assets/Scripts/Bots/AICombat.cs",
    "fileContent": "using Unity.VisualScripting;\nusing UnityEngine;\nusing UnityEngine.VFX;\n\n/// <summary>\n/// Handles:\n/// - Target aiming\n/// - Fire timing\n/// - Burst fire\n/// - Accuracy/spread\n/// - Weapon integration\n/// - Procedural recoil\n/// \n/// Does NOT:\n/// - Control movement\n/// - Decide AI states\n/// - Handle perception\n/// </summary>\npublic class AICombat : MonoBehaviour, IAIBehaviour\n{\n    public AIEntity entity;\n\n    [Header(\"References\")]\n    public Transform weaponMuzzle;\n\n    [Tooltip(\"Transform the rig aims toward\")]\n    public Transform weaponAimTarget;\n\n    public ProceduralWeaponMotion weaponMotion;\n\n    [Header(\"Combat\")]\n    public float attackRange = 25f;\n\n    [Tooltip(\"How quickly AI rotates aim target\")]\n    public float aimSpeed = 12f;\n\n    [Header(\"Fire Settings\")]\n    public bool automatic = true;\n\n    public float fireRate = 0.1f;\n\n    [Space]\n    public int burstCountMin = 2;\n    public int burstCountMax = 5;\n    [Space]\n    public float burstDelayMin = 0.08f;\n    public float burstDelayMax = 0.25f;\n\n    [Header(\"Accuracy\")]\n    [Tooltip(\"Lower = more accurate\")]\n    public float spreadRadius = 0.15f;\n\n    [Tooltip(\"Extra spread while moving\")]\n    public float movingSpreadMultiplier = 2f;\n\n    [Header(\"Projectile\")]\n    public GameObject bulletPrefab;\n    public float bulletSpeed = 100f;\n\n    [Header(\"Search Fire\")]\n    public bool suppressLastKnownPosition = true;\n    public float suppressionDurationMin;\n    public float suppressionDurationMax;\n    private float endSuppressionTime;\n\n    [Tooltip(\"Chance to continue firing at last known position\")]\n    [Range(0f, 1f)]\n    public float suppressionFireChance = 0.5f;\n\n    [Header(\"Debug\")]\n    public bool debugDrawShots = true;\n\n    public float nextFireTime;\n    public float nextBurstTime;\n\n    public int burstCount;\n    public int currentBurstShots;\n    public bool isBursting;\n\n    public bool isSuppressing;\n\n    private float lightOffTime = 0f;\n    private float muzzleLightDuration = .1f;\n\n    private Vector3 aimTargetRestPosition;\n\n    public void Initialize(AIEntity entity)\n    {\n        this.entity = entity;\n\n        burstCount = Random.Range(burstCountMin, burstCountMax);\n\n        if (weaponAimTarget != null)\n        {\n            aimTargetRestPosition = weaponAimTarget.position;\n        }\n    }\n\n    private void Update()\n    {\n        if (entity.currentState == AIState.Dead)\n        {\n            return;\n        }\n\n        switch (entity.currentState)\n        {\n            case AIState.Combat:\n                HandleCombat();\n                break;\n\n            case AIState.Suppress:\n                if (isSuppressing == false)\n                {\n                    isSuppressing = true;\n                    endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n                }\n                \n                if (isSuppressing == true)\n                {\n                    if (Time.time >= endSuppressionTime)\n                    {\n                        isSuppressing = false;\n                    }\n                    else\n                    {\n                        HandleSuppressionFire();\n                    }\n                }\n                break;\n\n            default:\n                ResetAimToRest();\n                break;\n        }\n\n        if (weaponMuzzle.GetComponent<Light>().enabled)\n        {\n            if (Time.time >= lightOffTime)\n            {\n                weaponMuzzle.GetComponent<Light>().enabled = false;\n            }\n        }\n    }\n\n    // --------------------------------------------------\n    // COMBAT\n    // --------------------------------------------------\n\n    private void HandleCombat()\n    {\n        Transform target = entity.perception.currentTarget;\n\n        if (target == null)\n        {\n            return;\n        }\n\n        float distance = Vector3.Distance(transform.position, target.position);\n\n        UpdateAim(target.position);\n\n        if (distance <= attackRange)\n        {\n            TryFire(target.position);\n        }\n    }\n\n    // --------------------------------------------------\n    // SUPPRESSION FIRE\n    // --------------------------------------------------\n\n    private void HandleSuppressionFire()\n    {\n        if (!suppressLastKnownPosition)\n        {\n            return;\n        }\n\n        if (Random.value > suppressionFireChance)\n        {\n            return;\n        }\n\n        UpdateAim(\n            entity.perception.lastKnownPosition\n        );\n\n        TryFire(\n            entity.perception.lastKnownPosition\n        );\n    }\n\n    // --------------------------------------------------\n    // AIMING\n    // --------------------------------------------------\n\n    private void UpdateAim(Vector3 targetPosition)\n    {\n        if (weaponAimTarget == null)\n        {\n            return;\n        }\n\n        Vector3 current = weaponAimTarget.position;\n\n        Vector3 smoothed = Vector3.Lerp(current, targetPosition, Time.deltaTime * aimSpeed);\n\n        weaponAimTarget.position = smoothed;\n    }\n\n    private void ResetAimToRest()\n    {\n        if (weaponAimTarget == null)\n        {\n            return;\n        }\n\n        Vector3 current = weaponAimTarget.position;\n        Vector3 smoothed = Vector3.Lerp(current, aimTargetRestPosition, Time.deltaTime * aimSpeed);\n\n        weaponAimTarget.position = smoothed;\n    }\n\n    // --------------------------------------------------\n    // FIRE CONTROL\n    // --------------------------------------------------\n\n    private void TryFire(Vector3 targetPosition)\n    {\n        if (Time.time < nextFireTime)\n        {\n            return;\n        }         \n\n        if (isBursting == true && currentBurstShots >= burstCount)\n        {\n            isBursting = false;\n            currentBurstShots = 0;\n            burstCount = Random.Range(burstCountMin, burstCountMax);\n            nextBurstTime = Time.time + Random.Range(burstDelayMin, burstDelayMax);\n        }\n\n        if (automatic)\n        {\n            FireShot(targetPosition);\n            // SET NEXT FIRE TIME\n            nextFireTime = Time.time + fireRate;\n        }\n        else\n        {\n            if (!isBursting && Time.time > nextBurstTime)\n            {\n                isBursting = true;\n            }\n\n            if (isBursting)\n            {\n                if (currentBurstShots < burstCount)\n                {\n                    FireShot(targetPosition);\n\n                    currentBurstShots++;\n                    // SET NEXT FIRE TIME\n                    nextFireTime = Time.time + fireRate;\n                }\n            }\n        }\n\n        \n    }\n\n    // --------------------------------------------------\n    // SHOOTING\n    // --------------------------------------------------\n    private void FireShot(Vector3 targetPosition)\n    {\n        Vector3 shotDirection = GetSpreadDirection(targetPosition);\n\n        // SPAWN PHYSICAL PROJECTILE\n        GameObject bullet = Instantiate(bulletPrefab, weaponMuzzle.position, Quaternion.identity);\n        bullet.transform.parent = null;\n        bullet.GetComponent<Rigidbody>().linearVelocity = shotDirection * bulletSpeed;\n\n        if (debugDrawShots)\n        {\n            Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n        }\n\n        weaponMuzzle.GetComponent<VisualEffect>().Play();\n        weaponMuzzle.GetComponent<Light>().enabled = true;\n        lightOffTime = Time.time + muzzleLightDuration;\n\n        // PROCEDURAL RECOIL\n        if (weaponMotion != null)\n        {\n            weaponMotion.Fire(1f);\n        }\n    }\n\n    private void FireRaycastShot(Vector3 targetPosition)\n    {\n        Vector3 shotDirection = GetSpreadDirection(targetPosition);\n\n        // RAYCAST HITSCAN\n        //This is okay for getting things setup, but I want to use a physical projectile instead of a raycast system\n        //TODO: Redo this section and instantiate a prefab bullet\n        if (Physics.Raycast(weaponMuzzle.position, shotDirection, out RaycastHit hit, attackRange))\n        {\n            AIHealth health = hit.collider.GetComponentInParent<AIHealth>();\n\n            if (health != null)\n            {\n                if (health.entity.team != entity.team)\n                {\n                    health.TakeDamage(20f);\n                }\n            }\n\n            if (debugDrawShots)\n            {\n                Debug.DrawLine(weaponMuzzle.position, hit.point, Color.red, 1f);\n            }\n        }\n        else\n        {\n            if (debugDrawShots)\n            {\n                Debug.DrawRay(weaponMuzzle.position, shotDirection * attackRange, Color.yellow, 1f);\n            }\n        }\n\n        weaponMuzzle.GetComponent<VisualEffect>().Play();\n        weaponMuzzle.GetComponent<Light>().enabled = true;\n        lightOffTime = Time.time + muzzleLightDuration;\n\n        // PROCEDURAL RECOIL\n        if (weaponMotion != null)\n        {\n            weaponMotion.Fire(1f);\n        }\n    }\n\n    // --------------------------------------------------\n    // SPREAD\n    // --------------------------------------------------\n\n    private Vector3 GetSpreadDirection(Vector3 targetPosition)\n    {\n        Vector3 direction = (targetPosition - weaponMuzzle.position).normalized;\n\n        float finalSpread = spreadRadius;\n\n        // Moving inaccuracy\n        if (entity.movement != null)\n        {\n            float speed = entity.movement.GetVelocityMagnitude();\n\n            if (speed > 0.1f)\n            {\n                finalSpread *= movingSpreadMultiplier;\n            }\n        }\n\n        Vector3 spreadOffset = Random.insideUnitSphere * finalSpread;\n\n        direction += spreadOffset;\n\n        return direction.normalized;\n    }\n\n    public void OverideSuppression()\n    {\n        isSuppressing = true;\n\n        endSuppressionTime = Time.time + Random.Range(suppressionDurationMin, suppressionDurationMax);\n    }\n}\n",
    "overwriteFile": true
  },
  "pluginIdentifier": "412407408"
}
```

### Tool
Tool call result:

```
"#path:C:\\Users\\jtemt\\Documents\\UnityProjects\\BattleOps\\Assets\\Scripts\\Bots\\AICombat.cs\n#type:file\n#status:created\n#fileEncoding:utf8\n#overwritten:true\n#lines:378\n#bytes:9880"
```

### Assistant
The file has been updated successfully. Let me verify the changes were applied correctly by reading the key sections.

&lt;/think&gt;


Done! The `AICombat.cs` file has been updated with the aim reset functionality. Here's a summary of the changes:

**Added:**
- `aimTargetRestPosition` field to store the starting aim position
- Caching of rest position in `Initialize()` 
- `default` case in the `Update()` switch that calls `ResetAimToRest()`
- New `ResetAimToRest()` method that smoothly lerps the aim target back to its rest position

Now when bots transition out of `Combat` or `Suppress` states (into `Idle`, `Patrol`, or `Search`), their weapon aim will smoothly return to the forward/resting position.
