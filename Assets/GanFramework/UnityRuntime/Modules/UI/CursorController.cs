// using GanFramework.Core;

// namespace GanFramework.UnityRuntime.UI
// {
//     public class CursorController
//     {
//         public bool IsNeedAutoLockCursor = true;

//         public bool IsShouldLockCursor()
//         {
            
//             if (IsNeedAutoLockCursor == false)
//                 return false;
//             foreach (var layer in UnLockedCursorLayers)
//             {
//                 if (IsLayerHasUIActive(layer))
//                     return false;
//             }
//             return true;
//         }

//     }
// }