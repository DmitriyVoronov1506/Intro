document.addEventListener('DOMContentLoaded', () => {

    const userRealName = document.getElementById("userRealName");

    userRealName.onclick = editableClick;
    userRealName.onkeydown = editableKeyDown;

    userRealName.onblur = e => {
        e.target.removeAttribute("contenteditable");

        if (e.target.savedValue != e.target.innerText) {
            fetch("/Auth/ChangeRealName?NewName=" + e.target.innerText).then(r => r.text())
                .then((t) => {

                    if (t != "No Errors" && t != "Name was updated!") {
                        e.target.innerText = e.target.savedValue;
                    }

                    alert(t)

                });
        }
    };

    const userLogin = document.getElementById("userLogin");

    if (!userLogin) throw "userLogin not found in DOM";

    userLogin.onclick = editableClick;
    userLogin.onblur = userLoginBlur;
    userLogin.onkeydown = editableKeyDown;

    const userEmail = document.getElementById("userEmail");

    if (!userEmail) throw "userEmail not found in DOM";

    userEmail.onclick = editableClick;
    userEmail.onblur = userEmailBlur;
    userEmail.onkeydown = editableKeyDown;

    const userPassword = document.getElementById("userPassword");

    if (!userPassword) throw "userPassword not found in DOM";

    userPassword.onclick = editableClick;
    userPassword.onblur = userPasswordBlur;
    userPassword.onkeydown = editableKeyDown;

    const avatar = document.getElementById("avatar");

    if (!avatar) throw "avatar not found in DOM";

    avatar.onchange = avatarChange;
});

function avatarChange(e) {

    if (e.target.files.length > 0) {

        const formData = new FormData();
        formData.append("userAvatar", e.target.files[0]);

        fetch("/Auth/ChangeAvatar", {
            method: "POST",
            body: formData
        }).then(r => r.json())
            .then(j => {
                if (j.status == "Ok") {
                    userLogo.src = "/img/UserImg/" + j.message;
                    imageBar.src = "/img/UserImg/" + j.message;
                }
                else {
                    alert(j.message);
                }
            });
    }
}

function editableClick(e) {
    e.target.setAttribute("contenteditable", true);
    e.target.savedValue = e.target.innerText;

    if (e.target.id == "userPassword") {
        e.target.innerText = "";
    }
}

function userLoginBlur(e) {

    e.target.removeAttribute("contenteditable");

    if (e.target.savedValue != e.target.innerText) {
        fetch("/Auth/ChangeLogin",
            {
                method: "POST",
                headers:
                {
                    //"Content-Type": "application/x-www-form-urlencoded" // [FromForm]
                    "Content-Type": "application/json"  // [FromBody]
                },
                body: JSON.stringify(e.target.innerText)
            })
            .then(r => r.json())
            .then(console.log);
    }
}

function editableKeyDown(e) {

    if (e.key == "Enter") {
        e.preventDefault();
        e.target.blur();
    }
}

function userEmailBlur(e) {

    e.target.removeAttribute("contenteditable");

    if (e.target.savedValue != e.target.innerText) {
        fetch("/Auth/ChangeEmail",
            {
                method: "PUT",
                headers:
                {
                    "Content-Type": "application/x-www-form-urlencoded" // [FromForm]
                },
                body: `NewEmail=${e.target.innerText}`
            })
            .then(r => r.json())
            .then(console.log);
    }
}

function userPasswordBlur(e) {
    e.target.removeAttribute("contenteditable");

    if (e.target.innerText.length < 3) {
        alert("Новый пароль не может иметь менее чем 3 символа");
        e.target.innerText = "New password";
    }
    else {
        fetch("/Auth/ChangePassword",
            {
                method: "POST",
                headers:
                {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(e.target.innerText)
            })
            .then(r => r.json())
            .then(alert);
    }

    e.target.innerText = "New password";
}