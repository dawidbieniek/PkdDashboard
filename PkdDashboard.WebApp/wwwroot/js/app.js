const observer = new MutationObserver(() => {
    let dialog = document.getElementById("components-reconnect-modal");
    if (dialog) {
        var shadowRoot = dialog.shadowRoot;
        if (shadowRoot) {
            var panel = shadowRoot.querySelector(
                ".components-reconnect-dialog"
            );
            if (panel) {
                panel.style.backgroundColor = "transparent";
                panel.style.backdropFilter = "blur(3px)";
            }
        }
    }
});

observer.observe(document.body, { childList: true, subtree: true });
