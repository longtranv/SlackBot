// Get references to the input elements
let nameInput = document.getElementById("nameInput");
let button = document.getElementById("submitButton");

button.addEventListener("click",async ()=>{
    try {
        const response = await fetch("https://localhost:44367/CheckPresence/submitform", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                Name: nameInput.value,
            })
        })
        button.disabled = true;
        const data = await response.json();
        console.log(data)
        alert(data)
    } catch (error) {
        console.log(error)
        alert(error)
    }
})
