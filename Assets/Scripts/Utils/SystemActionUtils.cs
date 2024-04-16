namespace Utils
{
    /// <summary>
    /// This class is used for c# System.Action
    /// </summary>
    public static class SystemActionUtils 
    {
        /// <summary>
        /// Helper function to clear events subscribed to parameter System.Action
        /// Loops through all assigned events and removes them from the event
        /// </summary>
        /// <param name="e">Event to clear</param>
        public static void ClearEvent(System.Action e)
        {
            if (e != null)
            {
                System.Delegate[] list = e.GetInvocationList();

                for (int i = 0; i < list.Length; i++)
                {
                    e -= (System.Action)list[i];
                }
            }
        }

        /// <summary>
        /// Overload for System.Action with type parameters
        /// 
        /// Helper function to clear events subcribed to parameter System.Action
        /// Loops through all assigned events and removes them from the event
        /// </summary>
        /// <typeparam name="T">Event type declared in System.Action, e.g T = GameObject in System.Action<GameObject></GameObject></typeparam>
        /// <param name="e">Event to clear</param>
        public static void ClearEvent<T>(System.Action<T> e)
        {
            if (e != null)
            {
                System.Delegate[] list = e.GetInvocationList();

                for (int i = 0; i < list.Length; i++)
                {
                    e -= (System.Action<T>)list[i];
                }
            }
        }

        /// <summary>
        /// Loops through System.Action e to check if given function/invoke is added.
        /// </summary>
        /// <param name="e">Event to loop through</param>
        /// <param name="check">The invoke event to check for</param>
        /// <returns>True if eventToCheckFor is already listening. False if e has no events or eventToCheckFor is not subscribed</returns>
        public static bool DoesEventExistInAction(System.Action e, System.Action check)
        {
            if (e == null)
            {
                return false;
            }

            System.Delegate[] list = e.GetInvocationList();

            for (int i = 0; i < list.Length; i++)
            {
                if (check == (System.Action)list[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loops through System.Action e to check if given function/invoke is added.
        /// </summary>
        /// <param name="e">Event to loop through</param>
        /// <param name="check">The invoke event to check for</param>
        /// <returns>True if eventToCheckFor is already listening. 
        /// False if e has no events or eventToCheckFor is not subscribed</returns>
        public static bool DoesEventExistInAction<T>(System.Action<T> e, System.Action<T> check)
        {
            if (e == null)
            {
                return false;
            }

            System.Delegate[] list = e.GetInvocationList();

            for (int i = 0; i < list.Length; i++)
            {
                if (check == (System.Action<T>)list[i])
                {
                    return true;
                }
            }

            return false;
        }
    } 
}