﻿document.addEventListener("DOMContentLoaded", function () {

    const buttonPublish = document.getElementById("button-publish");

    if (buttonPublish) buttonPublish.onclick = buttonPublishClick;

    loadArticles();

});

function buttonPublishClick(e) {

    const articleText = document.getElementById("article-text");
    if (!articleText) throw "article-text element not found";

    const picture = document.querySelector("input[name=picture]");
    if (!picture) throw "picture element not found";

    const txt = articleText.value;
    const authorId = articleText.getAttribute("data-author-id");
    const topicId = articleText.getAttribute("data-topic-id");

    console.log("Author ID: " + articleText.dataset.author);                      // Выводим всё из атрибутов Data
    console.log("Topic ID: " + topicId);
    console.log("Text: " + txt);
    console.log("Creation date: " + articleText.dataset.datetime);


    const formData = new FormData();

    formData.append('TopicId', topicId);
    formData.append('Text', txt);
    formData.append('AuthorId', authorId);

    formData.append('PictureFile', picture.files[0]);

    fetch("/api/article", {
        method: "POST",
        body: formData
    })
      .then(r => r.json())
      .then(j => {
          if (j.status == "Ok") {

              articleText.value = "";
              loadArticles();
          }        
          else alert(j.message);
      });
       
}

function loadArticles() {

    const articles = document.querySelector("articles");
    if (!articles) throw "articles element not found";

    const id = articles.getAttribute("topic-id");

    fetch(`/api/article/${id}`)
        .then(r => r.json())
        .then(j => {
            console.log(j);
            var html = "";
            const tpl = `<div style='border:1px solid salmon'>
                              <img src='/img/UserImg/{{avatar}}' style='max-height:7ch' />                   
                              <b>{{author}} @{{moment}}</b>:
                              <p>{{text}}</p>                       
                         </div>`;

            for (let article of j) {
                const moment = new Date(article.createdDate);
                html += tpl.replaceAll("{{author}}", article.author.realName)
                    .replaceAll("{{text}}", article.text)
                    .replaceAll("{{avatar}}", (article.author.avatar == "" || article.author.avatar == null ? "no-avatar.png" : article.author.avatar))
                    .replaceAll("{{moment}}", new Date(article.createdDate).toLocaleString("ru-RU"));
            } 

            articles.innerHTML = html;
        });
}

