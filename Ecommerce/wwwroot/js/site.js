function showToast(message, type) {
  type = type || 'success';
  var iconMap = { success: 'success', error: 'error', info: 'info' };
  Swal.fire({
    toast: true,
    position: 'bottom-end',
    icon: iconMap[type] || 'success',
    title: message,
    showConfirmButton: false,
    timer: 3200,
    timerProgressBar: true
  });
}

document.addEventListener("DOMContentLoaded", function () {
// 1. Grab the theme toggle button
const themeToggleBtn = document.getElementById("theme-toggle");

// 2. Check if the button exists on the current page
if (themeToggleBtn) {
    themeToggleBtn.addEventListener("click", function (e) {
        e.preventDefault(); // Prevent any default button behavior
        
        // 3. Toggle the 'dark' class on the <html> tag
        const isDark = document.documentElement.classList.toggle("dark");
        
        // 4. Save the user's preference to localStorage so it remembers across pages
        localStorage.setItem("theme", isDark ? "dark" : "light");
    });
}
});

document.addEventListener('DOMContentLoaded', function () {
  var body = document.body;
  var msg = body.getAttribute('data-toast-message');
  var type = body.getAttribute('data-toast-type');
  if (msg) { showToast(msg, type || 'success'); }
});
