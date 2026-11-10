# KKVR_HandyPoser

> 在**戀活！（Koikatsu）官方 VR** 中，直接用 VR 控制器抓取並調整角色身體各部位的姿勢。

[English](README.md)

> [!WARNING]
> 早期版本（v0.1.0），僅在有限環境測試過，可能還有問題，歡迎回報。

## 功能

- **抓取任意身體部位**：手、腳、手肘、膝蓋、肩膀、大腿、臀部。
- **按住拖曳、放開固定**：放開後部位會停在原位。
- **雙擊重置**：把已固定的部位還給動畫控制。
- 適用 H 場景中的**所有角色**，包含 3P 的第二位女性，也可選擇包含男性。
- 標記球顯示可抓的部位（白＝未操作、青＝已固定、黃＝可抓取、綠＝抓取中）。
- 固定的部位可選擇跟著角色移動（預設），或固定在房間中的絕對位置。
- 擺姿勢時會攔截抓取按鈕，避免觸發遊戲原本的動作。

## 需求

| | |
|---|---|
| 遊戲 | Koikatsu Party（Steam）或 コイカツ！（日版），需有官方 VR 執行檔（`Koikatsu Party VR.exe` / `KoikatuVR.exe`） |
| 模組載入器 | [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx) + HarmonyX，[HF Patch](https://github.com/ManlyMarco/KK-HF_Patch) 已內含 |
| VR | 任何支援 SteamVR、有兩支控制器的頭戴裝置 |

## 安裝

1. 從 [Releases](https://github.com/yumeyis/KKVR_HandyPoser/releases) 下載 `KKVR_HandyPoser_vX.Y.Z.zip`。
2. 解壓縮到遊戲根目錄（有 `BepInEx` 資料夾的那層）。
   DLL 應位於 `BepInEx/plugins/KKVR_HandyPoser/KKVR_HandyPoser.dll`。

## 操作方式

| 動作 | 預設操作 |
|---|---|
| 抓取部位 | 控制器靠近部位（標記變黃）後**按住 Grip** |
| 固定 | **放開 Grip** |
| 再次移動已固定部位 | 再抓一次 |
| 重置單一部位 | 在部位附近**雙擊 Grip** |
| 全部重置 | 鍵盤快捷鍵（需在設定中指定） |
| 開關擺姿勢功能 | 鍵盤快捷鍵（需在設定中指定） |

切換體位或 H 地點時，所有部位會自動重置。

## 設定

在非 VR 的本體遊戲按 **F1** 開啟 ConfigurationManager，或直接編輯
`BepInEx/config/yumeyis.KKVR_HandyPoser.cfg`。

| 設定項目 | 預設值 | 說明 |
|---|---|---|
| Enabled on start | `true` | H 場景開始時是否啟用擺姿勢功能 |
| Grab button | `Grip` | 抓取按鈕：Grip / Trigger / Touchpad / Menu |
| Grab radius | `0.08` | 控制器與部位之間可抓取的最大距離（公尺） |
| Double-click time | `0.3` | 判定為雙擊重置的最大間隔（秒） |
| Pin space | `Character` | `Character`：固定的部位跟著角色移動；`World`：固定在房間中的絕對位置 |
| Grabbable parts | `All` | 可抓取的部位群組：手／腳、手肘／膝蓋、肩膀／大腿、臀部 |
| Allow posing male | `true` | 是否允許調整男性角色 |
| Block game input while posing | `true` | 擺姿勢時不讓遊戲接收抓取按鈕 |
| Haptic feedback | `true` | 抓取／放開／重置時震動控制器 |
| Show markers | `false` | 在可抓取的部位顯示標記球 |
| Marker size | `0.03` | 標記球直徑（公尺） |
| Marker show distance | `0.4` | 控制器靠近部位多近時顯示標記（公尺） |
| Toggle posing | 未設定 | 鍵盤快捷鍵：開關擺姿勢功能 |
| Reset all parts | 未設定 | 鍵盤快捷鍵：重置所有已固定的部位 |
| Reset all buttons | `None` | 控制器按鈕組合：在同一支控制器上同時按住所有勾選的按鈕即可重置全部（例如勾選 Grip + Touchpad）。`None` 為停用 |

## 相容性

- **KK_SetParentVR**：兩者控制同一組 IK effector，請勿同時對同一肢體使用。
- **KoikatuVRAssistPlugin**：若與「Grab button」使用同一按鍵，請修改其中一方。
- 尚不支援非官方的本體 VR 模組（KK_MainGameVR）與 CharaStudio VR。輸入層已抽象化，日後可擴充。

## 從原始碼建置

請見 [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md)。

## 致謝

- 參考 [KK_SetParentVR](https://github.com/MayouKurayami/KK_SetParentVR) 的設計概念（未複製其程式碼）。
- 基於 [BepInEx](https://github.com/BepInEx/BepInEx) 與 [HarmonyX](https://github.com/BepInEx/HarmonyX)。

## 授權

[MIT](LICENSE)
