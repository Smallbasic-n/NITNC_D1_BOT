window.autoClick=()=>
{
    document.getElementById("loginform").submit();
    document.getElementById("test1").click();
    console.log("alpha");
}
window.getLocation = () => {
    navigator.geolocation.getCurrentPosition((position) => {
        console.log(`Latitude: ${position.coords.latitude}, Longitude: ${position.coords.longitude}`);
    });
}