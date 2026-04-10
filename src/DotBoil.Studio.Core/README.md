# DotBoil.Studio

Blazor Server tabanlı dinamik form builder ve headless CMS. Kod yazmadan form oluşturmanızı, API bağlantılı data tablolar tasarlamanızı ve sayfa yapılarını görsel olarak düzenlemenizi sağlar.

---

## Kurulum

```xml
<ProjectReference Include="..\DotBoil.Studio.Core\DotBoil.Studio.Core.csproj" />
```

---

## Konfigürasyon

```json
{
  "DotBoil": {
    "Studio": {
      "Persistence": {
        "ConnectionString": "server=localhost;uid=root;pwd=pass;database=Studio"
      }
    },
    "AuthServer": {
      "Url": "https://auth.example.com",
      "MenuEndpoint": "menus",
      "RefreshTokenEndpoint": "auth/refresh"
    },
    "ApiDataSources": [
      {
        "Name": "Ana API",
        "BaseUrl": "https://api.example.com"
      },
      {
        "Name": "Raporlama API",
        "BaseUrl": "https://reports.example.com"
      }
    ]
  }
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Studio.Persistence.ConnectionString` | `string` | Studio MySQL bağlantı dizesi |
| `AuthServer.Url` | `string` | AuthGuard API base URL'i |
| `AuthServer.MenuEndpoint` | `string` | Menü verisi çekilecek endpoint |
| `AuthServer.RefreshTokenEndpoint` | `string` | Token yenileme endpoint'i |
| `ApiDataSources[].Name` | `string` | API'nin görünen adı |
| `ApiDataSources[].BaseUrl` | `string` | API'nin base URL'i |

---

## Özellikler

### Form Builder

Sürükle-bırak arayüzüyle dinamik formlar tasarlayın:

- **Alan Tipleri:** Text, Textarea, Number, Password, Email, Checkbox, RadioButton, Select, DatePicker, ColorPicker, File Upload, HTML Editor
- **Veri Kaynakları:** Static liste veya API endpoint bağlantısı
- **Doğrulama Kuralları:** Zorunlu alan, min/max uzunluk, regex, özel mesajlar
- **Bağımlılık:** Bir alanın değerine göre başka alanların görünürlüğü
- **Koşullu Görünürlük:** `DependsOn` kuralları ile alan bağımlılıkları

### Data Table

API endpoint'lerine bağlı listeleme sayfaları:

- Sayfalandırma (sunucu taraflı)
- Sıralama (sütun bazlı)
- Badge kolonları (renk koşulları)
- Aksiyon butonları (yönlendirme veya API çağrısı)
- Onay modalı ile güvenli aksiyon

### Designer

- Görsel bileşen konfigürasyon paneli
- Sürükle-bırak sıralama
- İkon seçici
- Sayfa adı ve açıklaması

---

## Bileşen Tipleri

| Bileşen | Tip | Açıklama |
|---------|-----|----------|
| `TextField` | Input | Tek satır metin |
| `TextareaField` | Input | Çok satır metin |
| `NumericField` | Input | Sayısal değer |
| `PasswordField` | Input | Şifre (maskelenmiş) |
| `EmailField` | Input | E-posta adresi |
| `CheckBoxField` | Input | Onay kutusu |
| `RadioButtonField` | Input | Tekli seçim |
| `SelectField` | Input | Açılır liste |
| `DatePickerField` | Input | Tarih seçici |
| `ColorPickerField` | Input | Renk seçici |
| `FileUploadField` | Input | Dosya yükleme |
| `HtmlEditorField` | Input | Zengin metin editörü |
| `DataTableComponent` | Data | API bağlantılı veri tablosu |

---

## Form Konfigürasyonu Saklama

Form yapılandırmaları `UIConfig` entity'sinde JSON olarak saklanır:

```csharp
public interface IUIConfigService
{
    Task<UIConfig?> GetAsync(string name);
    Task<UIConfig?> GetAsync(int id);
    Task<UIConfig> CreateAsync(string name, BaseComponent component);
    Task UpdateAsync(int id, BaseComponent component);
    IQueryable<UIConfig> GetAll();
}
```

---

## Özel Bileşen Oluşturma

`BaseComponent`'tan türeyen yeni bileşenler tanımlayabilirsiniz:

```csharp
public class RatingField : BaseComponent
{
    public override string Category     => "Input";
    public override string Title        => "Rating";
    public override string ComponentIcon => Icons.Material.Rounded.Star;
    public override Type   RendererType => typeof(RatingFieldRenderer);

    [FieldProperty(1, "Maksimum Değer")]
    public int MaxValue { get; set; } = 5;

    [FieldProperty(2, "Renk")]
    public string Color { get; set; } = "Warning";
}
```

---

## IMenuUISettingsService

Studio sayfalarını menü öğelerine bağlamak için:

```csharp
public interface IMenuUISettingsService
{
    IQueryable<MenuUISettings> GetAll();
    Task<MenuUISettings?> GetByMenuIdAsync(int menuId);
    Task CreateOrUpdateAsync(int menuId, MenuUISettingsType type, string value);
}
```

---

## Migration

```bash
dotnet ef database update --project DotBoil.Studio.Core --startup-project DotBoil.Studio
```

---

## Bağımlılıklar

- `DotBoil` (core)
- `DotBoil.EFCore`
- `MudBlazor 9.0.0`
- `CodeBeam.MudBlazor.Extensions 9.0.1`
- `MySql.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`