import api from "./api";

/**
 * Fetches a list of documents (metadata) from the backend.
 * @returns {Promise<Array<object>>} Array of document info objects
 */
export const getDocuments = async () => {
  const response = await api.get("/documents");
  // Assuming the backend returns an array of DocumentInfoDto objects
  return response.data;
};

/**
 * Fetches details for a single document by its ID.
 * @param {string} id The ID of the document.
 * @returns {Promise<object>} Document info object
 */
export const getDocumentById = async (id) => {
  const response = await api.get(`/documents/${id}`);
  // Assuming the backend returns a single DocumentInfoDto object
  return response.data;
};

/**
 * Uploads a new document file along with its description.
 * @param {File} file The file object to upload.
 * @param {string} description The description for the document.
 * @returns {Promise<object>} Document info object for the newly created document
 */
export const uploadDocument = async (file, description) => {
  const formData = new FormData();
  formData.append("file", file); // Key must match [FromForm] parameter name in C#
  formData.append("description", description); // Key must match [FromForm] parameter name in C#

  // Make sure to send as multipart/form-data
  const response = await api.post("/documents/upload", formData, {
    headers: {
      // Axios might set this automatically for FormData, but explicitly setting is safer
      "Content-Type": "multipart/form-data",
    },
  });
  // Assuming the backend returns the DocumentInfoDto of the created document
  return response.data;
};

// Approve a document
export const approveDocument = async (id) => {
  try {
    const response = await api.post(`/documents/${id}/approve`);
    return response.data; // Returns updated DocumentInfoDto
  } catch (error) {
    console.error(
      "Error approving document:",
      error.response?.data || error.message
    );
    throw error.response?.data || new Error("Document approval failed");
  }
};

// Reject a document
export const rejectDocument = async (id, comments) => {
  try {
    // Pass comments in the request body. If comments is null/undefined, send empty object.
    const payload = comments ? { comments } : {};
    const response = await api.post(`/documents/${id}/reject`, payload);
    return response.data; // Returns updated DocumentInfoDto
  } catch (error) {
    console.error(
      "Error rejecting document:",
      error.response?.data || error.message
    );
    throw error.response?.data || new Error("Document rejection failed");
  }
};

// Add other document-related API calls here later (e.g., approve, reject, get history)
