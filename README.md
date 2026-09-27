# Unity AppsFlyer Service (UPM Package)

Package module tích hợp giải pháp phân bổ người dùng (**Attribution Tracking**), đo lường chuyển đổi (**In-App Events**), liên kết sâu (**Deep Linking**) và theo dõi doanh thu quảng cáo (**Impression-Level Ad Revenue**) từ **AppsFlyer** cho **Unity Core Framework**.

Được tích hợp sẵn với kiến trúc **Pluggable Service Bridge** (`Unity.Core`), tự động bắt doanh thu quảng cáo từ `Unity.Core.Services.Ads` (AppLovin MAX / AdMob) và gửi sang AppsFlyer.

---

## 🚀 Các Tính Năng Nổi Bật

1. **Attribution, Deep Linking & In-App Events**:
   - Tự động tích hợp với `Unity.Core.Services.Analytics.AnalyticsService` và `Unity.Core.Services.Tracking.TrackingService`.
   - Tự động hỗ trợ ATT (App Tracking Transparency) timeout trên iOS.

2. **Impression-Level Ad Revenue (ILR) Tracking**:
   - Tự động bắt `AdRevenueInfo` từ `TrackingService` và bắn `AppsFlyerAdRevenue.logAdRevenue()` chuẩn hoá theo mạng quảng cáo.

3. **Unity Editor Setup Tool**:
   - Menu: **`Unity Core > AppsFlyer > Setup & Configuration`**.
   - Quản lý và lưu `DevKey`, `AppID` (iOS) vào `Assets/Resources/Info.json`.

4. **IL2CPP Stripping Safe**:
   - Kèm file `link.xml` bảo vệ các symbol của AppsFlyer SDK khỏi bị strip khi build release với Managed Stripping Level = High.

---

## 📦 Cài Đặt Vào Dự Án

### Cách 1: Cài đặt qua Git URL trong Unity Package Manager
1. Mở Unity Editor: **Window** > **Package Manager**.
2. Nhấn vào dấu **`+`** (góc trên bên trái) > chọn **Add package from git URL...**
3. Nhập:
   ```text
   https://github.com/thoxuong92/com.unity.appsflyer.git
   ```

### Cách 2: Qua file `Packages/manifest.json`
Thêm dependency trỏ tới kho lưu trữ GitHub:
```json
{
  "dependencies": {
    "com.unity.core": "https://github.com/thoxuong92/com.unity.core.git",
    "com.unity.appsflyer": "https://github.com/thoxuong92/com.unity.appsflyer.git"
  }
}
```

---

## 🛠️ Hướng Dẫn Sử Dụng Code

Gameplay và UI gọi thông qua `Unity.Core.Services.Analytics` hoặc `Unity.Core.Services.Tracking`:

```csharp
using System.Collections.Generic;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.Tracking;

// Gửi In-App Event
AnalyticsService.LogEvent("af_purchase", new Dictionary<string, object>
{
    ["af_revenue"] = 4.99,
    ["af_currency"] = "USD",
    ["af_content_type"] = "gold_pack"
});
```

---

## 👨‍💻 Tác Giả & Bản Quyền
- **Tác giả**: **joukyuu**
- **Repository**: [thoxuong92/com.unity.appsflyer](https://github.com/thoxuong92/com.unity.appsflyer.git)
