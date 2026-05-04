# Yandex SDK Plugin (PluginYG2) — Инструкция для Claude

## Общая информация

Плагин для Unity, обеспечивающий интеграцию с Яндекс Играми и другими платформами. Основное API доступно через статический класс `YG2`.

## Инициализация

```csharp
using YG;
// Данные загружаются автоматически при старте
// После загрузки данных вызывается событие YG2.onGetSDKData
```

---

## Модули и API

### 1. Сохранения (Storage)

**Настройка класса сохранений:**

```csharp
namespace YG
{
    public partial class SavesYG
    {
        public int coins = 5;
        public List<ItemData> items = new List<ItemData>();
    }
}
```

**Использование:**

```csharp
YG2.saves.coins += 10;        // запись
Debug.Log(YG2.saves.coins);   // чтение
YG2.SaveProgress();           // сохранить
// YG2.LoadProgress()         // загрузить (обычно не нужно)
```

**Сброс и первая сессия:**

```csharp
YG2.SetDefaultSaves();        // сброс, вызовет onDefaultSaves + onGetSDKData
YG2.onDefaultSaves += () => { /* первый запуск */ };
```

---

### 2. Реклама

**Interstitial (полноэкранная между уровнями):**

```csharp
YG2.InterstitialAdvShow();
// События: onOpenInterAdv, onCloseInterAdv, onErrorInterAdv
// Проверки: YG2.nowInterAdv, YG2.isTimerAdvCompleted
```

**Rewarded (за вознаграждение):**

```csharp
// Способ 1 — коллбэк
YG2.RewardedAdvShow("reward_id", () => { coins += 10; });

// Способ 2 — событие
YG2.onRewardAdv += (id) => { if (id == "coin") coins++; };
YG2.RewardedAdvShow("coin");
```

**Sticky-баннеры (Яндекс Игры):**

```csharp
YG2.StickyAdActivity(true);   // показать
YG2.StickyAdActivity(false);  // скрыть
```

**Пауза при рекламе:**

- Автоматическая (Time.timeScale = 0, AudioListener.pause и т.д.)
- Событие: `YG2.onPauseGame` (bool)

---

### 3. Авторизация

```csharp
YG2.player.auth       // авторизован ли
YG2.player.name       // ник (unauthorized/anonymous если нет)
YG2.player.id         // ID пользователя
YG2.player.photo      // URL аватарки
YG2.OpenAuthDialog(); // открыть окно авторизации
```

---

### 4. Таблица лидеров (Leaderboards)

```csharp
YG2.SetLeaderboard("TechnoNameLB", 100);  // запись рекорда
// Time-тип:
YG2.SetLBTimeConvert("nameLB", timer);
```

---

### 5. Внутриигровые покупки (Payments)

```csharp
YG2.BuyPayments("product_id");  // открыть окно покупки
YG2.onPurchaseSuccess += (id) => { /* выдать товар */ };
YG2.onPurchaseFailed += (id) => { /* ошибка */ };

// Консумирование необработанных покупок (обязательно!)
YG2.ConsumePurchases();
YG2.ConsumePurchaseByID("id");
```

---

### 6. Локализация

```csharp
YG2.lang                     // текущий язык (ru, en...)
YG2.SwitchLanguage("en");    // переключить
YG2.onSwitchLang += (lang) => { /* обновить UI */ };
```

---

### 7. Данные окружения (EnvirData)

```csharp
YG2.envir.deviceType    // desktop, mobile, tablet, tv
YG2.envir.isDesktop     // bool
YG2.envir.language      // ru, en
YG2.envir.payload       // параметр из URL ?payload=value
```

---

### 8. Метрика (Metrica)

```csharp
YG2.MetricaSend("level_up");
YG2.MetricaSend("ui_click", new Dictionary<string, string> { {"button", "start"} });
```

---

### 9. Другие модули

**Оценка игры:**

```csharp
YG2.ReviewShow();
bool canShow = YG2.reviewCanShow;
YG2.onReviewSent += (sent) => { if (sent) GiveReward(); };
```

**Флаги (удалённая конфигурация):**

```csharp
string value = YG2.GetFlag("difficult");
if (YG2.TryGetFlagAsInt("power", out int power)) { }
```

**Серверное время:**

```csharp
long timeMs = YG2.ServerTime();
```

**Ярлык на рабочий стол:**

```csharp
YG2.GameLabelShowDialog();
bool canShow = YG2.gameLabelCanShow;
```

**Ссылки на игры:**

```csharp
YG2.OnDeveloperURL();
YG2.OnGameURL(gameID);
```

---

### 10. PlayerPrefs (переопределение)

```csharp
using PlayerPrefs = RedefineYG.PlayerPrefs;
// Далее стандартное использование PlayerPrefs
// Сохранять: YG2.SaveProgress() или PlayerPrefs.Save()
```

### 11. PlayerStats (быстрые сохранения int)

```csharp
YG2.SetState("coins", 100);
int coins = YG2.GetState("coins");
```

---

## Важные требования Яндекс Игр

 1. **SDK должен инициализироваться до начала игры**, после загрузки вызвать `LoadingAPI.ready()`
 2. **Авторизация** — только после явного действия пользователя, должен быть гостевой режим
 3. **Реклама** — только через SDK Яндекс Игр, только в логических паузах
 4. **Звук** — останавливаться при сворачивании
 5. **Реклама за вознаграждение** — бонус не должен быть обязательным для прохождения
 6. **Задержка перед рекламой** — не более 0.33 секунды
 7. **Покупки** — только через SDK, обязательно консумирование
 8. **Сохранения** — рекомендуется облачные через Storage + Authorization
 9. **Запрещены** внешние ссылки, сторонняя реклама, политика/религия/насилие/18+
10. **WebGL** — нет системного плеера, нет скроллбара браузера


