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

document.addEventListener("DOMContentLoaded", () => {
    const themeToggleBtn = document.getElementById("theme-toggle");

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener("click", () => {
            const isDark = document.documentElement.classList.toggle("dark");
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
