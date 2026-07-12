export function walkDom(html, action, {htmlOnly = true, doFirst = true} = {}) {
    const childType = htmlOnly ? "children" : "childNodes"; 

    if(doFirst) {
        if(action(html, [])) return;
    };

    const done = [0];
    let index = 0;
    let current = html;

    while(true) {
        if(done[index] < current[childType].length) {
            current = current[childType][done[index]]
            if(action(current, done)) {
                current = current.parentElement;
                done[index]++;
            } else {
                done.push(0);
                index++;
            }
        }
        else {
            done.pop();
            index--;
            done[index]++;
            if(index < 0) return;

            current = current.parentElement; 
        }
    }
}

export function getDomNode(html, path, {htmlOnly = true} = {}) {
    const childType = htmlOnly ? "children" : "childNodes";
    let current = html;

    for(const n of path) {
        current = current[childType][n];
    }

    return current;
}

const parser = new DOMParser();

export function stringToDom(str) {
    return parser.parseFromString(str, "text/html").body.firstElementChild;
}