using BombIt.Networking;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public Action<int> OnStartHostClicked;
    public Action<string, int> OnConnectClicked;
    public Action OnStopClicked;
    private GameObject lobbyPanel;
    private GameObject statusPanel;
    private InputField hostPortField;
    private InputField addressField;
    private InputField joinPortField;
    private Text statusText;
    private Text peerListText;
    public void Build()
    {
        EnsureEventSystem();
        var canvasGO = new GameObject("Lobby Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasGO.AddComponent<GraphicRaycaster>();
        lobbyPanel = CreatePanel(canvasGO.transform, "Lobby Panel");
        CreateLabel(lobbyPanel.transform, "Bomb It", 32);
        CreateLabel(lobbyPanel.transform, "Host a game", 20);
        hostPortField = CreateInputField(lobbyPanel.transform, "Port", "7777");
        CreateButton(lobbyPanel.transform, "Start Host", HandleStartHostClicked);
        CreateLabel(lobbyPanel.transform, "Join a game", 20);
        addressField = CreateInputField(lobbyPanel.transform, "Host address", "127.0.0.1");
        joinPortField = CreateInputField(lobbyPanel.transform, "Port", "7777");
        CreateButton(lobbyPanel.transform, "Connect", HandleConnectClicked);
        statusPanel = CreatePanel(canvasGO.transform, "Status Panel");
        statusText = CreateLabel(statusPanel.transform, "", 20);
        peerListText = CreateLabel(statusPanel.transform, "", 16);
        CreateButton(statusPanel.transform, "Stop / Disconnect", HandleStopClicked);
        statusPanel.SetActive(false);
    }
    public void ShowLobby()
    {
        lobbyPanel.SetActive(true);
        statusPanel.SetActive(false);
    }
    public void ShowStatus(string status, string peerList)
    {
        lobbyPanel.SetActive(false);
        statusPanel.SetActive(true);
        statusText.text = status;
        peerListText.text = peerList;
    }
    private void HandleStartHostClicked()
    {
        int port;
        if (!int.TryParse(hostPortField.text, out port))
        {
            port = NetworkConstants.defaultPort;
        }
        if (OnStartHostClicked != null)
        {
            OnStartHostClicked(port);
        }
    }
    private void HandleConnectClicked()
    {
        int port;
        if (!int.TryParse(joinPortField.text, out port))
        {
            port = NetworkConstants.defaultPort;
        }
        if (OnConnectClicked != null)
        {
            OnConnectClicked(addressField.text, port);
        }
    }
    private void HandleStopClicked()
    {
        if (OnStopClicked != null)
        {
            OnStopClicked();
        }
    }
    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
    private GameObject CreatePanel(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420, 400);
        rect.anchoredPosition = Vector2.zero;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }
    private Text CreateLabel(Transform parent, string labelText, int fontSize)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = fontSize + 10;
        var label = go.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        return label;
    }
    private InputField CreateInputField(Transform parent, string placeholderText, string defaultValue)
    {
        var go = new GameObject("InputField");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 36;
        var image = go.AddComponent<Image>();
        image.color = Color.white;
        var inputField = go.AddComponent<InputField>();
        inputField.targetGraphic = image;
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 4);
        textRect.offsetMax = new Vector2(-10, -4);
        var text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleLeft;
        inputField.textComponent = text;
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(go.transform, false);
        var placeholderRect = placeholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 4);
        placeholderRect.offsetMax = new Vector2(-10, -4);
        var placeholder = placeholderGO.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 16;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        placeholder.text = placeholderText;
        inputField.placeholder = placeholder;
        inputField.text = defaultValue;
        return inputField;
    }
    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.95f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        return button;
    }
}