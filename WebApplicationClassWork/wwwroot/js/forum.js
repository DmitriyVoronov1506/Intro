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

    fetch("/templates/topic.html")
        .then(r => r.text())
        .then(trTemplate => {

            var appHtml = "";

            for (let topic of j) {

                let tpl = trTemplate;

                for (let prop in topic) {
                    tpl = tpl.replaceAll(`{{${prop}}}`, topic[prop]);
                }
                appHtml += tpl;

                //appHtml +=
                //    trTemplate
                //        .replaceAll("{{title}}", topic.title)
                //        .replaceAll("{{description}}", topic.description)
                //        .replaceAll("{{id}}", topic.id);
            }

            elem.innerHTML = appHtml;

            //let collection = document.getElementsByClassName("topic");  // получаем по классу коллекцию дивов

            //for (let i = 0; i < collection.length; i++) {  // вешаем на все обработчики
            //    collection[i].addEventListener('click', () => showId(j[i].id));
            //}

            topicLoaded();

        });
}

async function topicLoaded() {

    for (let topic of document.querySelectorAll(".topic")) {
        topic.onclick = topicClick;
    }
}

function topicClick(e) {

    window.location = "/Forum/Topic/" + e.target.closest(".topic").getAttribute("data-id");
}

//function showId(id) {  // вывод на экран  ид
//    alert(id);
//}