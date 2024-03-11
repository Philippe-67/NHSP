function makeApiRequest() {
    var token = localStorage.getItem("jwtToken");

    fetch("https:localhost:6001", {
        method: "Get",
        headers: {
            'Authorization': "Bearer " + token
        }
    })
        .then(response => response.json())
        .then(data => console.log(data))
        .catch(error => console.error("Erreur :", error));
}
