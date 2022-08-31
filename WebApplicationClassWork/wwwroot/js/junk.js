document.addEventListener("DOMContentLoaded", () => {
    const junk = document.querySelector("junk");
    if (!junk) throw "Container <junk> not found";
    junk.innerHTML = "Тут будут удаленные сообщения";

    fetch("/api/article?del=true")
        .then(r => r.json())
        .then(j => {
            // console.log(j);
            let html = "";
          /*  const tpl = "<tr>{{moment}} {{topic}} {{text}} &#128472; <ins>&#x21ED;</ins> </tr>";*/

            const headerTable = "<tr><th>Moment</th><th>Topic</th><th>Text</th><th>Settings</th></tr>";
            var table = "";

            for (let article of j) {
                
                table += `<tr data-id=${article.id}><td>${formatDateIfDateToday(new Date(article.deleteMoment))}</td><td>${article.topic.title}</td><td>${article.text}</td><td>&#128472; <ins>&#x21ED;</ins></td></tr>`;
            }

            junk.innerHTML = `<table id='topics'><col style="width:20px"><col><col><col style="width:10px">${headerTable}${table}</table>`;

            if (typeof j[0] !== 'undefined') {
                junk.setAttribute("data-user-id", j[0].authorId);
            }

            onArticleLoaded();
        })
});

function onArticleLoaded() {
    for (let ins of document.querySelectorAll("td ins")) {
        ins.onclick = insClick;
    }
}

function insClick(e) {
    const uid = e.target.closest('tr').getAttribute('data-id');
    const thisTr = e.target.closest('tr');
    const junk = document.querySelector("junk");
    console.log(uid);
    fetch("/api/article?uid=" + uid, {
        method: "PURGE",
        headers: {
            "User-Id": junk.getAttribute('data-user-id')
        }
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.message == "Ok") {  // успешно восстановлена

                thisTr.parentNode.removeChild(thisTr);  // удаляем из таблицы 
            }
            else {  // ошибка восстановления (на бэке)
                alert(j.message);
            }
        });
}

function formatDateIfDateToday(deleteDate) {

    const todayDate = new Date();

    const checkIfToday = deleteDate.getDate() == todayDate.getDate()
        && deleteDate.getMonth() == todayDate.getMonth()
        && deleteDate.getFullYear() == todayDate.getFullYear();

    return checkIfToday ? deleteDate.toLocaleTimeString("ru-RU") : deleteDate.toLocaleString("ru-RU");
} 