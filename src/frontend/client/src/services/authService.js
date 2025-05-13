import api from "./api";

/**
 * Registers a new user.
 * @param {string} email
 * @param {string} password
 * @returns {Promise<any>} Backend response (structure depends on your API, could be simple message or user info)
 */
export const register = async (email, password) => {
  // The backend might throw specific errors (e.g., 400 Bad Request for duplicate email)
  // The response interceptor in api.js handles generic errors (like 401, 500)
  // Specific errors (like validation) will be passed through and should be handled in the component
  const response = await api.post("/account/register", { email, password });
  return response.data; // Return data for the component to use (e.g., success message)
};

/**
 * Logs in a user.
 * @param {string} email
 * @param {string} password
 * @returns {Promise<{token: string, user: object}>} Object containing JWT and user info
 */
export const login = async (email, password) => {
  const response = await api.post("/account/login", { email, password });
  // Expecting { token: '...', user: { id: '', email: '', roles: [] } }
  if (response.data && response.data.token && response.data.user) {
    localStorage.setItem("token", response.data.token);
    // Store user info as a string
    localStorage.setItem("user", JSON.stringify(response.data.user));
    return response.data; // Return token and user info
  } else {
    // This case should ideally be handled by backend returning non-2xx status
    // which would be caught by the interceptor or component's catch block.
    console.error("Login response missing token or user data:", response);
    throw new Error("Login failed: Invalid response from server.");
  }
};

/**
 * Logs out the current user by clearing local storage.
 */
export const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  // Optionally: Could also call a backend endpoint to invalidate the token server-side
  // await api.post('/account/logout');
  console.log("User logged out, token/user info removed from local storage.");
};
