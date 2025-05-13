import React, { createContext, useState, useEffect, useCallback } from "react";
import {
  login as apiLogin,
  logout as apiLogout,
  register as apiRegister,
} from "../services/authService";

// Create context with a default value of null
// Components consuming context need to check if the value is null before accessing its properties
export const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true); // State to track initial auth check

  // Effect to check for existing user session on component mount
  useEffect(() => {
    console.log("AuthProvider mounted, checking local storage...");
    try {
      const storedToken = localStorage.getItem("token");
      const storedUser = localStorage.getItem("user");

      if (storedToken && storedUser) {
        console.log(
          "Found token and user in storage, attempting to parse user..."
        );
        // TODO: Add token validation logic here (e.g., check expiry)
        const parsedUser = JSON.parse(storedUser);
        setUser(parsedUser);
        console.log("User state initialized from storage:", parsedUser);
      } else {
        console.log("No token or user found in storage.");
      }
    } catch (error) {
      // If parsing fails or token is invalid, clear storage
      console.error("Failed to initialize auth state from storage:", error);
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      setUser(null);
    } finally {
      // Finished initial check, allow rendering children
      setLoading(false);
      console.log("Auth loading finished.");
    }
  }, []); // Empty dependency array means run only on mount

  // Login function wrapper
  const login = useCallback(async (email, password) => {
    try {
      setLoading(true);
      const data = await apiLogin(email, password); // data is the AuthResponseDto from backend (camelCased)
      setUser(data.userInfo); // Update user state from response.data.userInfo
      console.log("Login successful, user state updated:", data.userInfo);
      return data; // Return full AuthResponseDto to component if needed
    } catch (error) {
      // Error handling is mostly done by interceptor/component
      setUser(null); // Ensure user state is cleared on login failure
      console.error("AuthProvider login callback error:", error);
      throw error; // Re-throw for the component to handle (e.g., show error message)
    } finally {
      setLoading(false);
    }
  }, []);

  // Logout function wrapper
  const logout = useCallback(() => {
    console.log("AuthProvider logout callback triggered.");
    apiLogout(); // Clears local storage via authService
    setUser(null); // Clear user state
  }, []);

  // Register function wrapper
  const register = useCallback(async (email, password) => {
    // Register function doesn't modify auth state directly
    // It just calls the API. Success/error message handled in component.
    try {
      setLoading(true);
      const response = await apiRegister(email, password);
      console.log(
        "Registration API call successful (response in component)",
        response
      );
      return response;
    } catch (error) {
      console.error("AuthProvider register callback error:", error);
      throw error; // Re-throw for component
    } finally {
      setLoading(false);
    }
  }, []);

  // Value provided by the context
  const value = {
    user,
    isAuthenticated: !!user,
    loading, // Provide loading state for initial check
    login,
    logout,
    register,
  };

  // Render children only after initial loading is complete
  // Alternatively, render a loading indicator while loading
  return (
    <AuthContext.Provider value={value}>
      {children}
      {/* {!loading ? children : <p>Loading application...</p>} */}
    </AuthContext.Provider>
  );
};
