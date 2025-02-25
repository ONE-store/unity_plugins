using UnityEngine;
using OneStore.Common.Internal;
using System;

namespace OneStore.Common
{
    /// <summary>
    /// The StoreEnvironment class is responsible for determining the type of store where the app is installed.
    /// </summary>
    public class StoreEnvironment
    {
        /// <summary>
        /// Represents the StoreEnvironment Java class.
        /// </summary>
        private static readonly AndroidJavaClass storeEnvironmentClass = new AndroidJavaClass(Constants.StoreEnvironment);

        /// <summary>
        /// Determines the store type where the app was installed.<br/>
        /// <br/>
        /// @return One of the following values: <br/>
        ///         - <see cref="StoreType.ONESTORE"/>: Installed from ONE Store or a trusted store. <br/>
        ///         - <see cref="StoreType.VENDING"/>: Installed from Google Play Store. <br/>
        ///         - <see cref="StoreType.ETC"/>: Installed from other stores. <br/>
        ///         - <see cref="StoreType.UNKNOWN"/>: Store information is unknown. <br/>
        /// </summary>
        /// <returns>A <see cref="StoreType"/> value representing the app's installation source.</returns>
        public static StoreType GetStoreType()
        {
            var storeTypeValue = storeEnvironmentClass.CallStatic<int>(
                Constants.StoreEnvironmentGetStoreTypeMethod,
                JniHelper.GetApplicationContext()
            );
            return Enum.IsDefined(typeof(StoreType), storeTypeValue) ? (StoreType)storeTypeValue : StoreType.UNKNOWN;
        }
    }
}
