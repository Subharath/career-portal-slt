(function () {
    const fab = document.getElementById("aiFab");
    const widget = document.getElementById("aiWidget");
    const closeBtn = document.getElementById("aiClose");
    const backBtn = document.getElementById("aiBackBtn");

    const questionView = document.getElementById("aiQuestionView");
    const answerView = document.getElementById("aiAnswerView");

    const answerQuestion = document.getElementById("aiAnswerQuestion");
    const answerText = document.getElementById("aiAnswerText");

    const questionItems = document.querySelectorAll(".ai-help-item");

    if (!fab || !widget || !closeBtn || !questionView || !answerView || !answerQuestion || !answerText) {
        return;
    }

    function openWidget() {
        widget.classList.add("show");
        widget.setAttribute("aria-hidden", "false");
    }

    function closeWidget() {
        widget.classList.remove("show");
        widget.setAttribute("aria-hidden", "true");
    }

    function showQuestions() {
        questionView.style.display = "block";
        answerView.style.display = "none";
        answerQuestion.textContent = "";
        answerText.innerHTML = "";
    }

    function showAnswer(question, answer) {
        questionView.style.display = "none";
        answerView.style.display = "block";
        answerQuestion.textContent = question;
        answerText.innerHTML = answer;
    }

    async function loadAnswer(questionKey, questionLabel) {
        showAnswer(questionLabel, "Loading answer...");

        try {
            const res = await fetch("/AI/GetAnswer", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ questionKey: questionKey })
            });

            if (!res.ok) {
                throw new Error("Request failed");
            }

            const data = await res.json();
            showAnswer(questionLabel, data.answer || "Sorry, I couldn't find an answer.");
        } catch (error) {
            showAnswer(questionLabel, "Oops — I couldn’t load the answer right now. Please try again.");
            console.error(error);
        }
    }

    fab.addEventListener("click", function () {
        if (widget.classList.contains("show")) {
            closeWidget();
        } else {
            openWidget();
        }
    });

    closeBtn.addEventListener("click", closeWidget);

    if (backBtn) {
        backBtn.addEventListener("click", showQuestions);
    }

    questionItems.forEach(item => {
        item.addEventListener("click", function () {
            const key = this.getAttribute("data-key");
            const label = this.getAttribute("data-label") || this.innerText.trim();
            loadAnswer(key, label);
        });
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            closeWidget();
        }
    });
})();