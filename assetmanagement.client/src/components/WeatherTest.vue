<script setup>
import { ref, onMounted } from 'vue'

const weatherData = ref([])

onMounted(async () => {
  try {
    const res = await fetch('/weatherforecast')
    weatherData.value = await res.json()
  } catch (err) {
    console.error('API error:', err)
  }
})
</script>

<template>
  <div>
    <h2>Weather Forecast Test</h2>
    <ul>
      <li v-for="item in weatherData" :key="item.date">
        {{ item.date }} - {{ item.temperatureC }}°C - {{ item.summary }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
  h2 {
    color: #42b983;
  }
</style>
