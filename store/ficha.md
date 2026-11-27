# Ficha de Cliperino en la Microsoft Store

Las imágenes están en `publish\store\listing\` (no van al repo).

## Precios y disponibilidad
- Mercados: todos.
- Visibilidad: pública.
- Precio: gratis.
- Publicación: lo antes posible después de la certificación.

## Propiedades
- Categoría: **Foto y video**.
- Política de privacidad: `https://cliperino.lostdou.dev/en/privacy.html`
- Sitio web: `https://cliperino.lostdou.dev`
- Contacto de soporte: `https://github.com/dou-tools/cliperino/issues`
- Declaraciones del producto: dejar las opciones por defecto.
- Requisitos del sistema (mínimo / recomendado):
  - Teclado: sí / sí. Mouse: sí / sí.
  - Micrófono: no / no (es opcional).
  - Memoria: 4 GB / 8 GB (Cliperino y ffmpeg usan unos 600 MB con 90 s de repetición, y el juego corre al mismo tiempo).
  - DirectX: versión 11 / versión 11 (la captura de pantalla usa Direct3D 11).
  - Memoria de video, procesador y gráficos: sin especificar.

## Clasificación por edades
Cuestionario de IARC. Tipo de app: utilidad o productividad (no es un juego). Respuestas:
- Violencia, sexo, lenguaje, drogas, apuestas: no.
- ¿Los usuarios pueden comunicarse o compartir contenido dentro de la app?: no (los clips se comparten fuera de la app).
- ¿Comparte la ubicación?: no.
- ¿Permite compras digitales?: no.
- ¿Da acceso sin restricciones a internet (navegador)?: no.

Debería quedar para todas las edades.

## Paquetes
Subir `publish\store\Cliperino-0.6.0.msix`. Familia de dispositivos: Windows 10/11 Desktop.

## Opciones de envío

### Por qué necesita runFullTrust
```text
Cliperino is a Win32 desktop app (.NET, Windows Forms) packaged as MSIX. It needs full trust to run in the system tray, register a global hotkey (RegisterHotKey), capture the screen with DXGI Desktop Duplication and Windows.Graphics.Capture, capture system audio with WASAPI loopback and the microphone, and run the bundled ffmpeg.exe to encode the video.
```

### Notas para la certificación
```text
Cliperino is an instant replay recorder that lives in the system tray. No account or sign-in is needed.

How to test:
1. Launch Cliperino. It starts recording the screen in the background and its tray icon turns red.
2. Press Ctrl+Alt+F10. The last 60 seconds are saved to Videos\Cliperino and a "Clip saved" notice appears at the top right of the screen.
3. Click the notice, or right-click the tray icon and choose "Edit clip…", to open the editor and trim the clip.

The package includes ffmpeg (GPL v3, license in the "licenses" folder) to encode the video. The app does not connect to the internet on its own; updates come from the Microsoft Store.
```

---

## Descripción de la Store: Español

### Descripción
```text
Cliperino graba en segundo plano mientras jugás. Cuando pasa algo bueno, apretás una tecla y guarda los últimos segundos. Sin cuenta, sin publicidad y sin nube: tus clips quedan en tu PC.

GUARDÁ LO QUE YA PASÓ
Cliperino tiene siempre listos desde 5 segundos hasta 20 minutos de partida (1 minuto por defecto). Apretás Ctrl+Alt+F10 y el clip queda en tu carpeta de Videos, en una subcarpeta por juego. El atajo lo podés cambiar por la tecla que quieras.

HECHO PARA JUGAR
• Usa el codificador de tu placa de video NVIDIA, AMD o Intel, dedicada o integrada, para que el juego siga fluido. Si no hay, usa el procesador.
• Graba solo la ventana del juego o la pantalla entera.
• Captura el sonido de toda la PC o solo el del juego (en Windows 11).
• Con el micrófono activado, tu voz va en su propia pista para sacarla o editarla aparte.

EDITÁ Y COMPARTÍ
Hacé clic en el aviso de "Clip guardado" y se abre el editor: recortá el clip, sacá tu voz o todo el audio, cortá en el cuadro exacto y comprimilo para que entre en 10 MB o 50 MB y mandarlo por Discord.

ORDENADO
Una carpeta por juego, con el nombre que elijas, y un límite de espacio opcional que manda los clips más viejos a la papelera.

PRIVADO DE VERDAD
Esta versión no se conecta a internet por su cuenta: ffmpeg ya viene incluido y las actualizaciones llegan por la Microsoft Store. Lo que grabás no sale de tu PC.

GRATIS Y DE CÓDIGO ABIERTO
Cliperino es gratis y su código está publicado en GitHub con licencia MIT.
```

### Novedades de esta versión
Dejar vacío en el primer envío.

### Características del producto
```text
Guarda los últimos segundos con una tecla, de 5 segundos a 20 minutos
```
```text
Codifica con la placa de video: NVIDIA, AMD o Intel
```
```text
Graba solo la ventana del juego o la pantalla entera
```
```text
Sonido de toda la PC o solo del juego
```
```text
Tu voz en su propia pista
```
```text
Editor para recortar en el cuadro exacto
```
```text
Comprime clips a 10 MB o 50 MB para Discord
```
```text
Una carpeta por juego y límite de espacio opcional
```
```text
Sin cuenta, sin publicidad y sin nube
```
```text
Gratis y de código abierto
```

### Capturas (Escritorio) y sus leyendas
1. `es\1-game.png`
   ```text
   Apretás Ctrl+Alt+F10 y se guarda lo que acaba de pasar.
   ```
2. `es\2-editor.png`
   ```text
   Recortá el clip y comprimilo para que entre en Discord.
   ```
3. `es\3-mic.png`
   ```text
   Tu voz va en su propia pista: sacala con un clic.
   ```
4. `es\4-settings.png`
   ```text
   Elegí qué grabar: la ventana del juego o la pantalla, con o sin micrófono.
   ```

### Logos de la Store
- Arte de póster 9:16 (1440×2160): `poster-1440x2160.png`
- Arte de caja 1:1 (2160×2160): `caja-2160x2160.png`
- Ícono de la app 1:1 (300×300): `icono-300x300.png`
- 1:1 de 150×150 y 71×71: vacíos (usa los del paquete).
- Arte 16:9 "super hero" (1920×1080, opcional): `hero-1920x1080.png`

### Descripción breve
```text
Repetición instantánea gratis para Windows. Graba en segundo plano mientras jugás y, cuando pasa algo bueno, apretás una tecla y guarda los últimos segundos. Sin cuenta, sin publicidad y sin nube.
```

### Términos de búsqueda
```text
repetición instantánea
```
```text
grabar juegos
```
```text
clips de juegos
```
```text
grabador de pantalla
```
```text
grabar partidas
```
```text
grabar pantalla
```
```text
replay
```

### Requisitos adicionales del sistema
Hardware mínimo:
```text
Windows 10 versión 2004 o posterior, de 64 bits
```
```text
Placa de video NVIDIA, AMD o Intel para codificar sin cargar el procesador (sin placa, usa el procesador)
```
```text
Unos 150 MB de disco por minuto de clip a 1080p y 60 FPS
```
Hardware recomendado:
```text
Windows 11, para grabar solo el sonido del juego
```

### Copyright e información de marcas
```text
© 2026 LostDou
```

### Términos de licencia adicionales
```text
Cliperino es software libre con licencia MIT: https://github.com/dou-tools/cliperino/blob/main/LICENSE
Incluye ffmpeg, que tiene su propia licencia (GPL v3). Su licencia y el enlace a su código fuente están en la carpeta "licenses" de la app.
Términos de uso: https://cliperino.lostdou.dev/terminos.html
```

### Desarrollado por
```text
LostDou
```

---

## Store listing: English

### Description
```text
Cliperino records in the background while you play. When something good happens, you press a key and it saves the last few seconds. No account, no ads, no cloud: your clips stay on your PC.

SAVE WHAT ALREADY HAPPENED
Cliperino always keeps from 5 seconds to 20 minutes of gameplay ready (1 minute by default). Press Ctrl+Alt+F10 and the clip lands in your Videos folder, in a subfolder per game. You can change the shortcut to any key you like.

MADE FOR GAMING
• Uses the encoder on your NVIDIA, AMD or Intel graphics card, dedicated or integrated, so your game stays smooth. If there isn't one, it uses the processor.
• Records just the game window or the whole screen.
• Captures the sound of the whole PC or just the game (on Windows 11).
• With the microphone on, your voice gets its own track so you can remove it or edit it separately.

EDIT AND SHARE
Click the "Clip saved" notice to open the editor: trim the clip, remove your voice or all the audio, cut on the exact frame and compress it to fit in 10 MB or 50 MB to send it on Discord.

TIDY
One folder per game, named the way you want, and an optional space limit that sends the oldest clips to the Recycle Bin.

TRULY PRIVATE
This version doesn't go online on its own: ffmpeg comes included and updates arrive through the Microsoft Store. What you record never leaves your PC.

FREE AND OPEN SOURCE
Cliperino is free and its source code is published on GitHub under the MIT license.
```

### What's new in this version
Leave empty on the first submission.

### Product features
```text
Saves the last few seconds with one key, from 5 seconds to 20 minutes
```
```text
Encodes on your graphics card: NVIDIA, AMD or Intel
```
```text
Records just the game window or the whole screen
```
```text
Sound from the whole PC or just the game
```
```text
Your voice on its own track
```
```text
Editor to trim on the exact frame
```
```text
Compresses clips to 10 MB or 50 MB for Discord
```
```text
One folder per game and an optional space limit
```
```text
No account, no ads, no cloud
```
```text
Free and open source
```

### Screenshots (Desktop) and captions
1. `en\1-game.png`
   ```text
   Press Ctrl+Alt+F10 and what just happened is saved.
   ```
2. `en\2-editor.png`
   ```text
   Trim the clip and compress it to fit on Discord.
   ```
3. `en\3-mic.png`
   ```text
   Your voice gets its own track: remove it in one click.
   ```
4. `en\4-settings.png`
   ```text
   Choose what to record: the game window or the screen, with or without your mic.
   ```

### Store logos
- 9:16 Poster art (1440×2160): `poster-1440x2160.png`
- 1:1 Box art (2160×2160): `caja-2160x2160.png`
- 1:1 App tile icon (300×300): `icono-300x300.png`
- 1:1 150×150 and 71×71: leave empty (the package icons are used).
- 16:9 Super hero art (1920×1080, optional): `hero-1920x1080.png`

### Short description
```text
Free instant replay for Windows. It records in the background while you play and, when something good happens, you press a key and it saves the last few seconds. No account, no ads, no cloud.
```

### Search terms
```text
instant replay
```
```text
game clips
```
```text
game recorder
```
```text
screen recorder
```
```text
gameplay recorder
```
```text
clip recorder
```
```text
replay buffer
```

### Additional system requirements
Minimum hardware:
```text
Windows 10 version 2004 or later, 64-bit
```
```text
NVIDIA, AMD or Intel graphics card to encode without loading the processor (without one, it uses the processor)
```
```text
About 150 MB of disk space per minute of clip at 1080p and 60 FPS
```
Recommended hardware:
```text
Windows 11, to record just the game's sound
```

### Copyright and trademark info
```text
© 2026 LostDou
```

### Additional license terms
```text
Cliperino is free software under the MIT license: https://github.com/dou-tools/cliperino/blob/main/LICENSE
It includes ffmpeg, which has its own license (GPL v3). Its license and a link to its source code are in the app's "licenses" folder.
Terms of use: https://cliperino.lostdou.dev/en/terms.html
```

### Developed by
```text
LostDou
```
