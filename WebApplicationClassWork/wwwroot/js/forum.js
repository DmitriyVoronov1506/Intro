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

    for (let topic of j) {

        elem.innerHTML += `<div class = 'topic' data-id='${topic.id}'>
        <b>${topic.title}</b><i>${topic.description}</i></div>`;
    }

}