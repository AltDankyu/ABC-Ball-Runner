#import "SwSKAdNetwork.h"
#import "SwTypeUtil.h"

void _swRegisterAppForAdNetwork() { if (@available(iOS 11.3, *)) {
    [SKAdNetwork registerAppForAdNetworkAttribution];
} else {
    // Fallback on earlier versions
} }

void _swUpdateConversionValue(int cv) { if (@available(iOS 14.0, *)) {
    NSInteger cvInteger = cv;
    [SKAdNetwork updateConversionValue:cvInteger];
} else {
    // Fallback on earlier versions
} }

char* _swGetSkAdNetworks() {
    NSMutableArray *skAdNetworks = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"SKAdNetworkItems"];

    if (!skAdNetworks) {
        return swStringCopy([@"" UTF8String]);
    }
    NSMutableArray *networksArray = [[NSMutableArray alloc] init];

    for (NSDictionary *networkDict in skAdNetworks){
        NSString *network = [networkDict objectForKey:@"SKAdNetworkIdentifier"];
        if (network) {
            [networksArray addObject:network];
        }
    }
    
    NSString *joined = [networksArray componentsJoinedByString:@","];
    return swStringCopy([joined UTF8String]);
}

char* _swGetAdvertisingAttributionReportEndpoint() {
    NSString *endpoint = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"NSAdvertisingAttributionReportEndpoint"];
    if (!endpoint) {
        swStringCopy([@"" UTF8String]);
    }

    return swStringCopy([endpoint UTF8String]);
}
