document.addEventListener("DOMContentLoaded", () => {

    const app = document.querySelector("app");

    if (!app) throw "Forum script: APP not found";

    loadTopics(app);

});

function loadTopics(elem) {

    fetch("/api/topic",                    // Eсли ничего не указывать то по умолчанию get, headers null, body null
        {
            method: "GET",
            headers: {
                "User-Id": "",
                "Culture": ""
            },
            body: null
        })
        .then(r => r.json())
        .then(j => {

            if (j instanceof Array) {

                showTopics(elem, j);
            }
            else {
                throw "loadTopics: Backend data invalid";
            }        
        });

}

function showTopics(elem, j) {

    //// шапка таблицы
    //let tableheader = "<tr><th>Number</th><th>Title</th><th>Description</th></tr>";

    //let table = "";

    //// счётчик 
    //let i = 0;

    //for (let topic of j) { // бежим по топикам
        
    //    ++i; // увеличиваем счётчик
    //    table += `<tr data-id='${topic.id}'><td>${i}</td><td>${topic.title}</td><td>${topic.description}</td></tr>`;
    //}

    //// записываем в elem
    //elem.innerHTML = `<table id='topics'><col style=\"width: 10px\">${tableheader}${table}</table>`;



    //var trTemplate = "<tr><td>*title</td><td>*descr</td></tr>";
    //var appHtml = "<table border=1>";
    //for (let topic of j) {
    //    appHtml +=
    //        trTemplate
    //            .replace("*title", topic.title)
    //            .replace("*descr", topic.description);
    //}
    //appHtml += "</table>";
    //elem.innerHTML = appHtml;


    fetch("/templates/topic.html")
        .then(r => r.text())
        .then(trTemplate => {

            var appHtml = "";

            for (let topic of j) {
                appHtml +=
                    trTemplate
                        .replace("{{title}}", topic.title)
                        .replace("{{description}}", topic.description)
                        .replace("{{id}}", topic.id);
            }

            elem.innerHTML = appHtml;

        });
 
}
