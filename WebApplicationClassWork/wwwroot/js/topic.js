document.addEventListener("DOMContentLoaded", function () {

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

    let tplPromis = fetch("/templates/article.html")

    const id = articles.getAttribute("topic-id");

    fetch(`/api/article/${id}`)
        .then(r => r.json())
        .then(async j => {
            console.log(j);
            var html = "";
            const tpl = await tplPromis.then(r => r.text());

            for (let article of j) {
                const moment = new Date(article.createdDate);
                html += tpl.replaceAll("{{author}}", (article.author.id == article.topic.authorId ? article.author.realName + " TC" : article.author.realName))
                    .replaceAll("{{text}}", article.text)
                    .replaceAll("{{avatar}}", (article.author.avatar == "" || article.author.avatar == null ? "no-avatar.png" : article.author.avatar))
                    .replaceAll("{{moment}}", new Date(article.createdDate).toLocaleString("ru-RU"))
                    .replaceAll("{{PictureFileForArticle}}", (article.pictureFile != null && article.pictureFile != "" ? `<img src='/img/ArticleImg/${article.pictureFile}' style='height:10ch; width:10ch; display: block; float: left'>` : ""))
                    .replaceAll("{{id}}", article.id);
            } 

            articles.innerHTML = html;

            onArticlesLoad();
        });
}

function onArticlesLoad() {

    for (let span of document.querySelectorAll(".article span")) {
        span.onclick = replyClick;
    }
}

function replyClick(e) {

    console.log(e.target.closest(".article").getAttribute("data-id"));
}