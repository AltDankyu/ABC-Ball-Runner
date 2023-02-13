-keeppackagenames # This will prevent collisions with other 3rd parties after obfuscation. For example, the same we had with Fyber. Reference: https://supersonicstudio.monday.com/boards/883112163/pulses/2144806504

-keep class com.supersonic.wisdom.library.api.listener.IWisdomSessionListener { *; }
-keep class com.supersonic.wisdom.library.api.listener.IWisdomInitListener { *; }
-keep class com.supersonic.wisdom.library.api.WisdomSDK { *; }
-keep class com.supersonic.wisdom.library.domain.events.dto.EventMetadataDto { *; }
-keep class com.supersonic.wisdom.library.api.dto.WisdomConfigurationDto {*;}
-keep class com.supersonic.wisdom.library.domain.events.dto.ExtraEventDetailsDto {*;}

#AppSet ID
-keep class com.google.android.gms.appset.* { *; }
-keep class com.google.android.gms.tasks.* { *; }

#GAID
-keep class com.google.android.gms.ads.identifier.* {public *;}
