const baseUrl = "https://localhost:7036/api/"
async function getQuestion() {
    const subject = document.getElementById("subject").value;
    const level = document.getElementById("levels").value;
    const language = document.getElementById("languages").value;

    const prompt = {
        "Subject": subject,
        "Level": level,
        "Language": language
    }

    const url = baseUrl + "GPT/GPTChat"
    const params = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(prompt)
    }
    const response = await fetch(url, params);
    if (response.ok) {
        let data = await response.json();
        data = JSON.parse(data);

        const questionsList = document.getElementById("questions");
        const question = document.createElement("li");
        const questionText = document.createTextNode(data["question"]);
        const answerText = document.createTextNode(data["answer"]);
        question.appendChild(questionText);
        question.appendChild(document.createElement("br"));
        question.appendChild(answerText);
        questionsList.appendChild(question);
    } else {
        console.log(errors);
    }
}
