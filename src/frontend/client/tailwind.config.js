/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}", // Include all JS/TS/JSX/TSX files in src
  ],
  safelist: [
    "bg-white", // Common class, okay to keep
    "bg-slate-50", // Main page background, good to keep
    // Removed bg-pink-500 as it was for testing
    // Add other classes here if they also face similar issues
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
