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

    // шапка таблицы
    let tableheader = "<tr><th>Number</th><th>Title</th><th>Description</th></tr>";

    let table = "";

    // счётчик 
    let i = 0;

    for (let topic of j) { // бежим по топикам
        
        ++i; // увеличиваем счётчик
        table += `<tr data-id='${topic.id}'><td>${i}</td><td>${topic.title}</td><td>${topic.description}</td></tr>`;
    }

    // записываем в elem
    elem.innerHTML = `<table id='topics'>${tableheader}${table}</table>`;

}