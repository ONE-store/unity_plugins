# ONEstore In-app Plugin for Unity

## Overview

ONE store **In-app Integration Library *v1.3.1*** is a service that sells and charges products implemented in Android apps to users using ONE store's authentication and payment system, and settles them with the developers.

In order to pay for in-app products, it must be linked with the ONE store service (OSS) app, and the OSS app works with the ONE store payment server to conduct payments for in-app products.

### Caution

These are required libraries for using in-app purchases or check licenses.

* OneStoreCorpPlugins/Common
* OneStoreCorpPlugins/Authentication

## Do you use a proguard?

**It's already obfuscated and in aar, so add the package to the proguard rules.**

```text
# Core proGuard rules
-keep class com.gaa.sdk.base.** { *; }
-keep class com.gaa.sdk.auth.** { *; }

# Purchasing proGuard rules
-keep class com.gaa.sdk.iap.** { *; }

# Licensing proGuard rules
-keep class com.onestore.extern.licensing.** { *; }
```

## Include external dependencies

The In-app integration Unity Library is distributed with the [EDM4U(External Dependency Manager for Unity)](https://github.com/googlesamples/unity-jar-resolver).
This library is intended for use by any Unity plugin that requires access to Android-specific libraries. It provides Unity plugins the ability to declare dependencies, which are then automatically resolved and copied into your Unity project.

## How do I use In-app module?

### Authentication module

```csharp
using OneStore.Auth;

new OneStoreAuthClientImpl().LaunchSignInFlow((signInResult) => {
    if (signInResult.IsSuccessful())
        // Sign in succeeded.
    else
        // Sign in failed.
});
```

### Purchasing module

```csharp
using OneStore.Purchasing;

IPurchaseCallback callback = new IPurchaseCallback() {
    // implements method
};

// License key for your app registered in the ONE store Developer Center.
var licenseKey = "...";
var purchaseClient = new PurchaseClientImpl(licenseKey);
purchaseClient.Initialize(callback);
```

Refer to the [IAP documentation](https://onestore-dev.gitbook.io/dev/tools/tools/v21/12.-unity-sdk-v21#id-12.unity-sdkv21-14) for more information.

### Licensing module

```csharp
using OneStore.Alc;

ILicenseCheckCallback callback = new ILicenseCheckCallback() {
    // implements method
}

// License key for your app registered in the ONE store Developer Center.
var licenseKey = "...";
var licenseChecker = new OneStoreAppLicenseCheckerImpl(licenseKey);
licenseChecker.Initialize(callback);
```

Refer to the [ALC documentation](https://onestore-dev.gitbook.io/dev/tools/tools/alc/unity-alc-sdk-v2-1) for more information.

## Change Note

* 2025-03-10
    * Fix exception handling bug when using `getApplicationEnabledSetting()`
* 2025-02-25
    * Enhanced developer option features  
    * Added `StoreEnvironment.getStoreType()` API
* 2023-12-05
    * Fixed a bug where the `PurchaseClientImpl.QueryPurchases()` request was not responding when no purchases were found
    * Change the folder structure
    * Remove the AAR physical file and apply the gradle dependency.
    * [EDM4U(External Dependency Manager for Unity)](https://github.com/googlesamples/unity-jar-resolver) to enforce Gradle dependencies is mandatorily distributed with it.
    * `sdk-configuration-xx` is deprecated.
* 2023-05-18
    * Fixed [issues#5](https://github.com/ONE-store/onestore_iap_release/issues/5)
* 2023-01-10
    * Exception handling when the purchase data is null when calling consume and acknowlege API.
    * Track the connection status of the service and control it through queue management, even if you request the API multiple times in a short time.
* 2022-11-10
    * Release to samples with the integrated in-app SDK v1.1.0 for the unity.

# License

```text
Copyright 2023 One store Co., Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); 
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, 
software distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and
limitations under the License.
```
