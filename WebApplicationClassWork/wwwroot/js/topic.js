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

    const id = articles.getAttribute("topic-id");

    fetch(`/api/article/${id}`)
        .then(r => r.json())
        .then(j => {
            console.log(j);
            var html = "";
            const tpl = `<div style='border: 3px solid lightgray; box-shadow: 0 0 4px 5px lightblue; margin-bottom: 1em;
                         padding: 10px; border-radius: 5% 40% 5% 5%;; overflow: auto; background-color:lightgreen'>
                             <div style='display: block; float: left'>
                                 <img src='/img/UserImg/{{avatar}}' style='height:8ch; width:8ch; 
                                 border:1px solid grey; border-radius: 30%; background: white' />
                             </div>
                             <div style='float: none; overflow: auto; padding-left: 10px'>
                                 <b>{{author}}</b><br/> {{moment}}
                                 <hr style='border: 1px solid blue; border-radius: 2px;'>
                             </div>
                             {{PictureFileForArticle}}
                             <div style='float: none; overflow: auto; padding-left: 15px;'>
                                 <p>{{text}}</p>
                             </div>
                         </div>`;

            for (let article of j) {
                const moment = new Date(article.createdDate);
                html += tpl.replaceAll("{{author}}", (article.author.id == article.topic.authorId ? article.author.realName + " TC" : article.author.realName))
                    .replaceAll("{{text}}", article.text)
                    .replaceAll("{{avatar}}", (article.author.avatar == "" || article.author.avatar == null ? "no-avatar.png" : article.author.avatar))
                    .replaceAll("{{moment}}", new Date(article.createdDate).toLocaleString("ru-RU"))
                    .replaceAll("{{PictureFileForArticle}}", (article.pictureFile != null && article.pictureFile != "" ? `<img src='/img/ArticleImg/${article.pictureFile}' style='height:10ch; width:10ch; display: block; float: left'>` : ""));
            } 

            articles.innerHTML = html;
        });
}

