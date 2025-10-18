import { defineConfig } from 'unocss'
import presetWind4 from '@unocss/preset-wind4'

export default defineConfig({
    content: {
        filesystem: [
            "*.{html,js,ts,jsx,tsx,razor,cshtml}",
            "**/*.{html,js,ts,jsx,tsx,razor,cshtml}",
            "**/**/*.{html,js,ts,jsx,tsx,razor,cshtml}",
        ]
    },
    presets: [
        presetWind4(),
    ],
});