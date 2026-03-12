(function () {
    const fab = document.getElementById("aiFab");
    const widget = document.getElementById("aiWidget");
    const closeBtn = document.getElementById("aiClose");
    const sendBtn = document.getElementById("aiSend");
    const textBox = document.getElementById("aiText");
    const msgs = document.getElementById("aiMessages");

    if (!fab || !widget || !closeBtn || !sendBtn || !textBox || !msgs) {
        // Widget not present on this page
        return;
    }

    function openWidget() {
        widget.style.display = "block";
        widget.setAttribute("aria-hidden", "false");
        setTimeout(() => textBox.focus(), 50);
    }

    function closeWidget() {
        widget.style.display = "none";
        widget.setAttribute("aria-hidden", "true");
    }

    function addMsg(type, text) {
        const div = document.createElement("div");
        div.className = "ai-msg " + type;
        div.textContent = text;
        msgs.appendChild(div);
        msgs.scrollTop = msgs.scrollHeight;
    }

    function removeTypingIfExists() {
        const last = msgs.querySelector(".ai-msg.bot:last-child");
        if (last && last.textContent === "Typing...") last.remove();
    }

    async function send() {
        const q = (textBox.value || "").trim();
        if (!q) return;

        addMsg("user", q);
        textBox.value = "";
        addMsg("bot", "Typing...");

        try {
            const res = await fetch("/ai/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: q })
            });

            const data = await res.json();
            removeTypingIfExists();
            addMsg("bot", data.reply || "Sorry, I couldn't generate a reply.");
        } catch (e) {
            removeTypingIfExists();
            addMsg("bot", "Oops — I couldn’t reach the AI service. Please try again.");
        }
    }

    fab.addEventListener("click", () => {
        if (widget.style.display === "block") closeWidget();
        else openWidget();
    });

    closeBtn.addEventListener("click", closeWidget);
    sendBtn.addEventListener("click", send);

    textBox.addEventListener("keydown", (e) => {
        if (e.key === "Enter") send();
        if (e.key === "Escape") closeWidget();
    });
})();