// Author              :    Kwan Wai Teng
// Modified By         :    
// Company             :    Higanbanana Studios
// Create Time         :    16th April 2024
// Update Time         :    16th April 2024
// Class Description   :    Wrapper for the functionalities of Coroutine Manager Pro
// ===============================================================================
using System.Collections;

namespace Service.CoroutineSvc
{
    public interface ICoroutineService
    {
        public void StartCoroutine(IEnumerator cr);
        void StopCoroutine(ICoroutineHandle cr);
    }
}