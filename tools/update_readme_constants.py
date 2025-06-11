import re
from pathlib import Path

FORMATTER_PATH = Path("Formatter.cs")
README_PATH = Path("README.md")

START_MARKER = "[//]: # (VARIABLES_DESCRIPTIONS_START)"
END_MARKER = "[//]: # (VARIABLES_DESCRIPTIONS_END)"

def extract_constants():
    constants = []
    pattern = re.compile(r'public const string\s+\w+\s*=\s*"(?P<key>{[^"]+})";\s*//\s*(?P<desc>.+)')
    
    with FORMATTER_PATH.open(encoding="utf-8") as f:
        for line in f:
            match = pattern.search(line)
            if match:
                key = match.group("key")
                desc = match.group("desc")
                constants.append(f"`{key}`: {desc}")
    
    # Add backslashes to all except the last
    for i in range(len(constants) - 1):
        constants[i] += "\\"
    
    return constants

def update_readme(constants):
    content = README_PATH.read_text(encoding="utf-8")
    
    start_idx = content.find(START_MARKER)
    end_idx = content.find(END_MARKER)

    if start_idx == -1 or end_idx == -1 or start_idx > end_idx:
        raise ValueError("Start or end marker not found or in wrong order.")

    before = content[:start_idx + len(START_MARKER)]
    after = content[end_idx:]

    new_block = "\n" + "\n".join(constants) + "\n\n"
    updated_content = before + new_block + after

    README_PATH.write_text(updated_content, encoding="utf-8")
    print("README.md successfully updated.")

def main():
    try:
        constants = extract_constants()
        update_readme(constants)
    except Exception as e:
        print(f"Error: {e}")
        exit(1)

if __name__ == "__main__":
    main()
