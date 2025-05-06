let isRefreshing = false;
let refreshPromise = null;
function notify(message, type = 'info', duration = 2000, topOffset = 20) {
    document.body.classList.add('notify-active');
    const color = {
        info: '#2196F3',
        success: '#4CAF50',
        warning: '#FFC107',
        error: '#F44336'
    }[type] || '#333';

    const toast = document.createElement('div');
    toast.textContent = message;
    toast.style.cssText = `
        position: fixed;
        top: ${topOffset}px;          /* offset tùy biến */
        right: 20px;
        background: ${color};
        color: white;
        padding: 10px 20px;
        border-radius: 4px;
        font-size: 14px;
        z-index: 9999;
        box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        opacity: 0;
        transform: translateY(-20px);
        transition: opacity 0.3s ease, transform 0.3s ease;
    `;
    document.body.appendChild(toast);

    // Show animation (slide xuống)
    requestAnimationFrame(() => {
        toast.style.opacity = '1';
        toast.style.transform = 'translateY(0)';
    });

    // Tự ẩn sau `duration` ms
    setTimeout(() => {
        document.body.classList.remove('notify-active');
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(-20px)';  // slide lên khi ẩn
        toast.addEventListener('transitionend', () => toast.remove());
    }, duration);
}


async function performTokenRefresh() {
    try {
        const refreshResponse = await fetch('https://API20250506035221.azurewebsites.net/api/auth/refresh-token', {
            method: 'POST',
            credentials: 'include'
        });

        if (!refreshResponse.ok) throw new Error('Refresh failed');
        const data = await refreshResponse.json();
        console.log('New token:', data);
        // Lưu thông tin mới vào sessionStorage
        sessionStorage.setItem('accessToken', data.accessToken);
        sessionStorage.setItem('fullName', data.fullname); // Đảm bảo key đúng
        sessionStorage.setItem("avatar_url", "https://th.bing.com/th/id/R.b3a1e1a27386019b0be253c09cb3701b?rik=rCmgbtux1pK1rA&pid=ImgRaw&r=0");
        sessionStorage.setItem('role', data.role);

        const event = new Event('userDataUpdated');
        window.dispatchEvent(event);

        return data.accessToken;
    } catch (error) {
        notify("Bạn cần đăng nhập để truy cập tính năng này!", 'error');
        setTimeout(() => {
            logout(); 
        }, 1000);
        throw error;
    }
}

export async function requestAccessToken() {
    if (isRefreshing) {
        return refreshPromise;
    }

    isRefreshing = true;
    refreshPromise = performTokenRefresh()
        .finally(() => {
            isRefreshing = false;
        });

    return refreshPromise;
}

export async function fetchWithAuth(url, options = {}) {
    // Clone options để tránh mutation
    const authOptions = {
        ...options,
        headers: { ...options.headers }
    };

    const currentToken = sessionStorage.getItem('accessToken');
    if (currentToken) {
        authOptions.headers.Authorization = `Bearer ${currentToken}`;
    }
    try {
        let response = await fetch(url, authOptions);
        console.log("response", response);

        if (response.status === 401) {
            try {
                const newToken = await requestAccessToken();

                const retryOptions = {
                    ...authOptions,
                    headers: {
                        ...authOptions.headers,
                        Authorization: `Bearer ${newToken}`
                    }
                };

                return fetch(url, retryOptions);
            } catch (error) {
                return Promise.reject(error);
            }
        }
        if (response.status === 403) {
            notify("Bạn không có quyền truy cập tính năng này!", 'warning');
            setTimeout(() => {
                logout(); 
            }, 1000);
            return Promise.reject(new Error('Access denied'));
        }

        return response;
    } catch (error) {
        console.error('Error in fetchWithAuth:', error);
    }
}

export async function logout() {
    try {
        await fetch('https://API20250506035221.azurewebsites.net/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        });
    } catch (error) {
        console.error('Logout error:', error);
    } finally {
        sessionStorage.clear();
        window.location.href = '/wireless-charging/auth';
    }
}