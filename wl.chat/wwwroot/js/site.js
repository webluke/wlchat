window.scrollChatToBottom = function () {
    var chatContainer = document.querySelector('.chat-container');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
};

window.focusElement = (element) => {
    if (element) element.focus();
};

window.toggleSidebar = function (open) {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    if (open) {
        sidebar.classList.add('sidebar-open');
    } else {
        sidebar.classList.remove('sidebar-open');
    }
}

document.addEventListener('click', function (e) {
    if (
        window.innerWidth <= 700 &&
        sidebar.classList.contains('sidebar-open') &&
        !sidebar.contains(e.target) &&
        !sidebarToggle.contains(e.target)
    ) {
        sidebar.classList.remove('sidebar-open');
    }
});

window.highlightCode = () => {
    Prism.highlightAll();
};