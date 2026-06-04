// FlowState theme persistence. Light is the default; user choice is stored in localStorage.
window.flowstate = {
  getTheme() {
    return localStorage.getItem('fs-theme') || 'light';
  },
  applyStoredTheme() {
    document.documentElement.setAttribute('data-theme', this.getTheme());
  },
  toggleTheme() {
    const next = this.getTheme() === 'dark' ? 'light' : 'dark';
    localStorage.setItem('fs-theme', next);
    document.documentElement.setAttribute('data-theme', next);
    return next;
  }
};

// Apply immediately on script load to avoid a flash of the wrong theme.
window.flowstate.applyStoredTheme();
