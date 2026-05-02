document.documentElement.classList.add("js-enabled");

const root = document.documentElement;
const themeToggle = document.getElementById("themeToggle");
const themeIcon = themeToggle.querySelector("i");
const savedTheme = localStorage.getItem("dashboard-theme");
const systemPrefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;

const initialTheme = savedTheme === "dark" || savedTheme === "light"
    ? savedTheme
    : (systemPrefersDark ? "dark" : "light");
root.setAttribute("data-theme", initialTheme);

function syncThemeIcon() {
    const darkMode = root.getAttribute("data-theme") === "dark";
    themeIcon.className = darkMode ? "fa-solid fa-sun" : "fa-solid fa-moon";
}

syncThemeIcon();

themeToggle.addEventListener("click", () => {
    const nextTheme = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
    root.setAttribute("data-theme", nextTheme);
    localStorage.setItem("dashboard-theme", nextTheme);
    syncThemeIcon();
});