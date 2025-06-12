document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("chat-form");
    const input = document.getElementById("chat-message");
    const responseDiv = document.getElementById("chat-response");

    if (form) {
        form.addEventListener("submit", function (e) {
            e.preventDefault();

            const formData = new FormData();
            formData.append("message", input.value);

            fetch("/Chat/AskAjax", {
                method: "POST",
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        responseDiv.classList.remove("d-none");
                        responseDiv.innerText = data.answer;
                        input.value = "";
                    } else {
                        responseDiv.classList.remove("d-none");
                        responseDiv.classList.add("alert-danger");
                        responseDiv.innerText = data.error;
                    }
                })
                .catch(() => {
                    responseDiv.classList.remove("d-none");
                    responseDiv.classList.add("alert-danger");
                    responseDiv.innerText = "A apărut o eroare la trimiterea mesajului.";
                });
        });
    }
});
