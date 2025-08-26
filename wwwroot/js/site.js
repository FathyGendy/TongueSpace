// Dark Mode Toggle Functionality

// Check for saved theme preference or default to 'light'
const currentTheme = localStorage.getItem('theme') || 'light';

// Apply theme on page load
document.documentElement.setAttribute('data-theme', currentTheme);

// Update toggle button text based on current theme
function updateToggleButton() {
    const toggleBtn = document.querySelector('.theme-toggle');
    if (toggleBtn) {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        toggleBtn.innerHTML = currentTheme === 'dark' 
            ? '<i class="bi bi-sun-fill me-1"></i>Light Mode' 
            : '<i class="bi bi-moon-fill me-1"></i>Dark Mode';
    }
}

// Theme toggle function
function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateToggleButton();
}

// Initialize theme toggle when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Update button text on page load
    updateToggleButton();
    
    // Add click event to theme toggle button
    const toggleBtn = document.querySelector('.theme-toggle');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', toggleTheme);
    }
});

// Optional: Listen for system theme changes
if (window.matchMedia) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    // Only apply system theme if user hasn't set a preference
    if (!localStorage.getItem('theme')) {
        document.documentElement.setAttribute('data-theme', mediaQuery.matches ? 'dark' : 'light');
        updateToggleButton();
    }
    
    // Listen for system theme changes
    mediaQuery.addEventListener('change', function(e) {
        // Only apply system theme if user hasn't set a preference
        if (!localStorage.getItem('theme')) {
            document.documentElement.setAttribute('data-theme', e.matches ? 'dark' : 'light');
            updateToggleButton();
        }
    });
}