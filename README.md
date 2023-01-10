# ONEstore In-app Plugin for Unity

## Overview

ONE store **In-app Integration Library *v1.1.1*** is a service that sells and charges products implemented in Android apps to users using ONE store's authentication and payment system, and settles them with the developers.

In order to pay for in-app products, it must be linked with the ONE store service (OSS) app, and the OSS app works with the ONE store payment server to conduct payments for in-app products.

### Caution

These are required libraries for using in-app purchases or check licenses.

* com.onestorecorp.core
* com.onestorecorp.auth

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

Refer to the [IAP documentation](https://dev.onestore.co.kr/wiki/ko/doc/04-sdk-37552583.html) for more information.

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

Refer to the [ALC documentation](https://dev.onestore.co.kr/wiki/ko/doc/unity-alc-sdk-v2-39945604.html) for more information.

## Change Note

* 2023-01-10
    * Exception handling when the purchase data is null when calling consume and acknowlege API.
    * Track the connection status of the service and control it through queue management, even if you request the API multiple times in a short time.
* 2022-11-10
    * Release to samples with the integrated in-app SDK v1.1.0 for the unity.
