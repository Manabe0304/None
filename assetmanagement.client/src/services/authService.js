import axios from "axios"

const API_URL = "https://localhost:5001/api/auth"

export const login = async (username, password) => {
    const response = await axios.post(`${API_URL}/login`, {
        username,
        password
    })

    return response.data
}