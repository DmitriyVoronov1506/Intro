document.addEventListener("DOMContentLoaded", function () {

    const buttonPublish = document.getElementById("button-publish");

    if (!buttonPublish) throw "button-publish element not found";

    buttonPublish.onclick = buttonPublishClick;

});

function buttonPublishClick(e) {

    const articleText = document.getElementById("article-text");

    if (!articleText) throw "article-text element not found";

    const txt = articleText.value;

    console.log("Author ID: " + articleText.dataset.author);                      // Выводим всё из атрибутов Data
    console.log("Topic ID: " + articleText.dataset.topic);
    console.log("Text: " + txt);
    console.log("Creation date: " + articleText.dataset.datetime);
     
}