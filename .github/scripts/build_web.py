import html
import json
import re
import sys
from pathlib import Path

SITE = "https://cliperino.lostdou.dev/"
PAGES = {"index.html": "index.html", "privacidad.html": "privacy.html", "terminos.html": "terms.html"}
LOCALES = {"es": "es_AR", "en": "en_US"}
DOWNLOAD = "https://github.com/dou-tools/cliperino/releases/latest/download/Cliperino-Setup.exe"
APP = {
    "es": {
        "description": "Repetición instantánea gratis para Windows: graba en segundo plano y guarda los últimos segundos de tu partida con un atajo. Sin cuenta, para cualquier GPU y con clips listos para Discord.",
        "features": [
            "Repetición instantánea: guarda los últimos segundos o minutos con un atajo",
            "Funciona con placas NVIDIA, AMD o Intel, o con el procesador",
            "Editor para recortar clips",
            "Compresión para Discord: hasta 10 MB o 50 MB",
            "Micrófono en una pista de audio aparte",
            "Una carpeta por juego",
            "Límite de espacio que manda los clips viejos a la papelera",
        ],
    },
    "en": {
        "description": "Free instant replay for Windows: it records in the background and saves the last seconds of your game with a shortcut. No account, works with any GPU and makes clips ready for Discord.",
        "features": [
            "Instant replay: saves the last seconds or minutes with a shortcut",
            "Works with NVIDIA, AMD or Intel graphics cards, or the processor",
            "Editor to trim clips",
            "Discord compression: up to 10 MB or 50 MB",
            "Microphone on its own audio track",
            "One folder per game",
            "Space limit that sends old clips to the Recycle Bin",
        ],
    },
}


def page_url(language, name):
    path = "" if name in ("index.html",) else name
    return SITE + ("en/" if language == "en" else "") + path


def closing_tag(text, tag, start):
    depth = 1
    for match in re.compile(rf"<(/?){tag}\b[^>]*>", re.I).finditer(text, start):
        if match.group(0).endswith("/>"):
            continue
        depth += -1 if match.group(1) else 1
        if depth == 0:
            return match.start(), match.end()
    raise ValueError(f"<{tag}> sin cerrar")


def translate_text(text):
    pattern = re.compile(r'<([a-zA-Z][\w-]*)\b([^>]*?)\sdata-en="([^"]*)"([^>]*)>')
    parts, position = [], 0
    while match := pattern.search(text, position):
        start, end = closing_tag(text, match.group(1), match.end())
        parts += [text[position:match.start()], f"<{match.group(1)}{match.group(2)}{match.group(4)}>", html.unescape(match.group(3)), text[start:end]]
        position = end
    return "".join(parts + [text[position:]])


def translate_attributes(text):
    def tag(match):
        result = match.group(0)
        for name, value in re.findall(r'\sdata-en-([a-z-]+)="([^"]*)"', result):
            result = re.sub(rf'\sdata-en-{name}="[^"]*"', "", result)
            if re.search(rf'\s{name}="', result):
                result = re.sub(rf'(\s{name}=")[^"]*"', lambda m: f'{m.group(1)}{value}"', result)
            else:
                result = result[:-1] + f' {name}="{value}">'
        return re.sub(r"\s+>$", ">", result)

    return re.sub(r'<[a-zA-Z][^>]*\sdata-en-[a-z-]+="[^>]*>', tag, text)


def relocate(text):
    def path(match):
        value = match.group(2)
        if value in PAGES:
            value = "./" if value == "index.html" else PAGES[value]
        elif not re.match(r"^(?:[a-z]+:|#|/|\.)", value):
            value = "../" + value
        return f'{match.group(1)}"{value}"'

    return re.sub(r'(\s(?:src|href|poster)=)"([^"]*)"', path, text)


def without_translations(text):
    return re.sub(r'\sdata-en(?:-[a-z-]+)?="[^"]*"', "", text)


def structured_data(language, page):
    clean = without_translations(page)
    plain = lambda fragment: html.unescape(re.sub(r"<[^>]+>", "", fragment)).strip()
    faq = re.findall(r"<details>\s*<summary[^>]*>(.*?)</summary>\s*<p[^>]*>(.*?)</p>\s*</details>", clean, re.S)
    home = page_url(language, "index.html")
    data = {
        "@context": "https://schema.org",
        "@graph": [
            {
                "@type": "SoftwareApplication",
                "@id": SITE + "#app",
                "name": "Cliperino",
                "url": home,
                "description": APP[language]["description"],
                "applicationCategory": "MultimediaApplication",
                "operatingSystem": "Windows 10, Windows 11",
                "offers": {"@type": "Offer", "price": "0", "priceCurrency": "USD"},
                "downloadUrl": DOWNLOAD,
                "image": SITE + ("img/social-en.jpg" if language == "en" else "img/social.jpg"),
                "screenshot": [SITE + ("img/en/editor.webp" if language == "en" else "img/editor.webp"), SITE + ("img/en/settings.webp" if language == "en" else "img/ajustes.webp")],
                "featureList": APP[language]["features"],
                "inLanguage": ["es", "en"],
                "author": {"@type": "Person", "name": "lostdou", "url": "https://lostdou.dev"},
                "sameAs": ["https://github.com/dou-tools/cliperino"],
            },
            {"@type": "WebSite", "@id": home + "#site", "url": home, "name": "Cliperino", "inLanguage": language},
            {
                "@type": "FAQPage",
                "@id": home + "#faq",
                "inLanguage": language,
                "mainEntity": [{"@type": "Question", "name": plain(q), "acceptedAnswer": {"@type": "Answer", "text": plain(a)}} for q, a in faq],
            },
        ],
    }
    body = json.dumps(data, ensure_ascii=False, indent=2).replace("</", "<\\/")
    return '  <script type="application/ld+json">\n' + body + "\n  </script>\n"


def with_structured_data(language, page):
    if 'class="hero"' not in page:
        return page
    page = re.sub(r'  <script type="application/ld\+json">.*?</script>\n', "", page, flags=re.S)
    return page.replace("  </head>", structured_data(language, page) + "  </head>", 1)


def english(name, page):
    page = translate_text(page)
    page = translate_attributes(page)
    page = re.sub(r'\s*<article data-lang="es">.*?</article>', "", page, flags=re.S)
    page = page.replace('<article data-lang="en" hidden>', "<article>")
    page = page.replace('<html lang="es">', '<html lang="en">', 1)
    page = relocate(page)
    target = page_url("en", PAGES[name])
    page = re.sub(r'(<link rel="canonical" href=")[^"]*"', rf'\g<1>{target}"', page)
    page = re.sub(r'(<meta property="og:url" content=")[^"]*"', rf'\g<1>{target}"', page)
    page = page.replace(f'<meta property="og:locale" content="{LOCALES["es"]}">', f'<meta property="og:locale" content="{LOCALES["en"]}">')
    page = page.replace(f'<meta property="og:locale:alternate" content="{LOCALES["en"]}">', f'<meta property="og:locale:alternate" content="{LOCALES["es"]}">')
    return with_structured_data("en", page)


def write(path, content):
    if not path.exists() or path.read_text(encoding="utf-8") != content:
        path.write_text(content, encoding="utf-8", newline="")
        print(f"{path}")


def main(web):
    output = web / "en"
    output.mkdir(exist_ok=True)
    for name, target in PAGES.items():
        source = web / name
        spanish = with_structured_data("es", source.read_text(encoding="utf-8"))
        write(source, spanish)
        write(output / target, english(name, spanish))


if __name__ == "__main__":
    main(Path(sys.argv[1] if len(sys.argv) > 1 else "web"))
