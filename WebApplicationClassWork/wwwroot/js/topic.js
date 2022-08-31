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
    let replyId = articleText.getAttribute("data-reply-id");
    if (replyId.length < 5) replyId = "";

    console.log("Author ID: " + articleText.dataset.author);                      // Выводим всё из атрибутов Data
    console.log("Topic ID: " + topicId);
    console.log("Text: " + txt);
    console.log("Creation date: " + articleText.dataset.datetime);


    const formData = new FormData();

    formData.append('TopicId', topicId);
    formData.append('Text', txt);
    formData.append('AuthorId', authorId);
    formData.append('ReplyId', replyId);
    formData.append('PictureFile', picture.files[0]);

    fetch("/api/article", {
        method: "POST",
        body: formData
    })
      .then(r => r.json())
      .then(j => {
          if (j.status == "Ok") {

              articleText.value = "";
              articleText.setAttribute("data-reply-id", "");
              loadArticles();
          }        
          else alert(j.message);
      });
       
}

function loadArticles() {

    const articles = document.querySelector("articles");
    if (!articles) throw "articles element not found";

    let tplPromis = fetch("/templates/article.html")

    const articleText = document.getElementById("article-text");
    const authorId = (articleText)
        ? articleText.getAttribute("data-author-id")
        : "-";

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
                    .replaceAll("{{id}}", article.id)
                    .replaceAll("{{reply}}", (article.replyId == null ? "" : `<b>${article.reply.author.realName}</b>: ` + article.reply.text.substring(0, 13) + (article.reply.text.length > 13 ? "..." : "")))
                    .replaceAll("{{picture}}", (article.pictureFile == null || article.pictureFile == "" ? "no-picture.png" : article.pictureFile))
                    .replaceAll("{{replyTitle}}", `Article: ${getReplyText(j, article.replyId)}\r\nAuthor: ${getReplyAuthorName(j, article.replyId)}.\r\nCreated Date: ${new Date(getReplyCreatedDate(j, article.replyId)).toLocaleString("ru-RU")}`)
                    .replace("{{del-display}}", (article.authorId == authorId ? "inline-block" : "none"))
                    .replaceAll("{{edit-display}}", (ChangeButtonCheck(j, article.authorId, article.replyId, article.id) ? "inline-block" : "none"));
            }

            articles.innerHTML = html;

            onArticlesLoad();
        });
}

function onArticlesLoad() {

    for (let span of document.querySelectorAll(".article span")) {
        span.onclick = replyClick;
    }

    for (let del of document.querySelectorAll(".article del")) {
        del.onclick = deleteClick; 
    }
}

function replyClick(e) {

    const id = e.target.closest(".article").getAttribute("data-id");
    const articleText = document.getElementById("article-text");
    if (!articleText) throw "article-text element not found";
    articleText.setAttribute("data-reply-id", id);
    articleText.focus();
}

function getReplyText(arr, replyId) {

    for (article of arr) {

        if (article.id == replyId) {
            return article.text;
        }
    }
}

function getReplyAuthorName(arr, replyId) {

    for (article of arr) {

        if (article.id == replyId) {
            return article.author.realName;
        }
    }
}

function getReplyCreatedDate(arr, replyId) {

    for (article of arr) {

        if (article.id == replyId) {
            return article.createdDate;
        }
    }
}

function ChangeButtonCheck(j, artAuthorId, artReplyId, artId) {
    const articleText = document.getElementById("article-text");
    const authorId = (articleText) ? articleText.getAttribute("data-author-id") : "-";

    if (artAuthorId == authorId) {
        for (let article of j) {
            if (artReplyId == article.id) {
                return true; 
            }
            if (artId == article.replyId) {
                return false; 
            }
        }

        if (artReplyId == null) {
            return true; 
        }
    }
    else {
        return false; 
    }
}

function deleteClick(e) {
    const id = e.target.closest(".article").getAttribute("data-id");
    console.log(id);
} 