# AI Usage Guidelines

## Nae

- It is strictly prohibited to use generative AI to:
  - Generate "art", even for prototypes. Shitty programmer art is encouraged for this purpose instead.
    - no AI 3d models
    - no AI images/sprites/textures
    - no AI audio/music/sfx
    - no AI text dialogs
  - Generate these .md documentations such as the one you're reading right now.
- It is disencouraged, but allowed under certain conditions (such as prototyping and boilerplate) to:
  - Copy and paste AI generated code without modifying or thorouglhy checking as long as you include this crystal clear disclaimer at the top:

  ```cs
        /*
    *   202608231454 (YMDHM)
    *   DISCLAIMER: CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
    *   AGENT: CLAUDE.AI SONNET 5 (whatever you're using)
    */
  ```

## Aye

- It is allowed to use LLMs to:
  - Refactor code
  - Fix bugs, or at least try to
  - Write boilerplate code
  - Fix typos and inconsistencies
  - use the outputs of `code_extractor.py` to code review it
  - generate mockup data
  - structure data models and DTOs
