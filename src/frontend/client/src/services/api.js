import axios from "axios";

// Determine the API base URL
// Option 1: Use Vite environment variable (requires setup in .env file)
// const API_BASE_URL = import.meta.env.VITE_API_URL;
// Option 2: Hardcode for development (make sure port matches your backend launchSettings.json)
const API_BASE_URL = "http://localhost:5147/api"; // Updated to match actual backend URL

if (!API_BASE_URL) {
  console.error("API_BASE_URL is not defined. Please configure it.");
}

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor to add JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    console.error("Request interceptor error:", error);
    return Promise.reject(error);
  }
);

// Response interceptor for handling common errors like 401 Unauthorized
api.interceptors.response.use(
  (response) => response, // Simply return successful responses
  (error) => {
    console.error("Response interceptor error:", error.response || error);
    if (error.response && error.response.status === 401) {
      console.warn(
        "Unauthorized request. Clearing token and redirecting to login."
      );
      // Clear potentially invalid token/user data
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      // Redirect to login page
      // Avoid using useNavigate here as it's outside component context
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    } else {
      // You might want to handle other common errors here (e.g., 403 Forbidden, 500 Server Error)
      // For now, just re-throw the error for the calling function to handle
    }
    // Important: Always return a rejected promise to allow specific error handling in components
    return Promise.reject(error);
  }
);

export default api;
