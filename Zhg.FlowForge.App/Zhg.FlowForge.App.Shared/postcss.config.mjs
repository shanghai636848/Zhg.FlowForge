// postcss.config.mjs
import UnoCSS from '@unocss/postcss'

export default {
    plugins: [
        UnoCSS({
            configFile: './uno.config.mjs',
        }),
    ],
}